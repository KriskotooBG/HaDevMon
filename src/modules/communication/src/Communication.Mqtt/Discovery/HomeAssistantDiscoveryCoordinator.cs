using Communication.Abstractions.Entities;
using Communication.Mqtt.Configuration;
using Communication.Mqtt.Connection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Communication.Mqtt.Discovery
{
    internal sealed class HomeAssistantDiscoveryCoordinator
    {
        private const string OnlinePayload = "online";

        private readonly IMqttClient _client;
        private readonly MqttConnection _connection;
        private readonly MqttDiscoveryPublisher _discoveryPublisher;
        private readonly MqttOptions _options;
        private readonly ILogger<HomeAssistantDiscoveryCoordinator> _logger;

        private readonly Lock _entitySync = new();
        private readonly SemaphoreSlim _publishGate = new(1, 1);

        private EntityDescriptor[] _entities = [];



        public HomeAssistantDiscoveryCoordinator(
            IMqttClient client,
            MqttConnection connection,
            MqttDiscoveryPublisher discoveryPublisher,
            IOptions<MqttOptions> options,
            ILogger<HomeAssistantDiscoveryCoordinator> logger)
        {
            _client = client;
            _connection = connection;
            _discoveryPublisher = discoveryPublisher;
            _options = options.Value;
            _logger = logger;

            _connection.ConnectionReadyAsync += OnConnectionReadyAsync;
            _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        }

        public async Task RegisterEntitiesAsync(IReadOnlyCollection<EntityDescriptor> entities, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entities);

            lock (_entitySync)
            {
                _entities = [..entities];
            }

            await _connection.WaitUntilReadyAsync(cancellationToken);
            await PublishDiscoveryAsync(cancellationToken);
        }

        private async Task OnConnectionReadyAsync(CancellationToken cancellationToken)
        {
            await SubscribeToHomeAssistantStatusAsync(cancellationToken);

            //Even if we miss HA's birth message, discovery is offered again whenever the MQTT connection becomes ready.
            await PublishDiscoveryAsync(cancellationToken);
        }

        private async Task SubscribeToHomeAssistantStatusAsync(CancellationToken cancellationToken)
        {
            var subscribeOptions = new MqttClientFactory()
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter
                    .WithTopic(_options.HomeAssistantStatusTopic)
                    .WithAtMostOnceQoS()
                )
                .Build();

            await _client.SubscribeAsync(subscribeOptions, cancellationToken);
            _logger.LogInformation("Subscribed to Home Assistant MQTT status topic {Topic}", _options.HomeAssistantStatusTopic);
        }

        private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            if (!string.Equals(args.ApplicationMessage.Topic, _options.HomeAssistantStatusTopic,StringComparison.Ordinal))
                return Task.CompletedTask;

            var payload = args.ApplicationMessage.ConvertPayloadToString();
            if (!string.Equals(payload, OnlinePayload, StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            _logger.LogInformation("Home Assistant MQTT birth detected.");

            // Don't publish QoS 1 messages inline from MQTTnet's receive callback. Process the synchronization independently.
            Task.Run(HandleHomeAssistantBirthAsync, CancellationToken.None);
            return Task.CompletedTask;
        }

        private async Task HandleHomeAssistantBirthAsync()
        {
            try
            {
                await DelayForHomeAssistantBirthAsync();
                await _connection.WaitUntilReadyAsync(CancellationToken.None);
                await PublishDiscoveryAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to handle Home Assistant MQTT birth.");
            }
        }

        private async Task DelayForHomeAssistantBirthAsync()
        {
            var maximumMilliseconds = _options.HomeAssistantBirthDelayMax.TotalMilliseconds;
            if (maximumMilliseconds <= 0) return;

            var delay = TimeSpan.FromMilliseconds(Random.Shared.NextDouble() * maximumMilliseconds);
            _logger.LogDebug("Waiting {Delay} before republishing Home Assistant discovery", delay);

            await Task.Delay(delay);
        }

        private async Task PublishDiscoveryAsync(CancellationToken cancellationToken)
        {
            EntityDescriptor[] entities;
            lock (_entitySync) { entities = _entities; }
            if (entities.Length == 0) return;

            var device = _connection.Device;
            if (device is null) return;

            await _publishGate.WaitAsync(cancellationToken);

            try
            {
                await _discoveryPublisher.PublishAsync(device, entities, cancellationToken);
            }
            finally
            {
                _publishGate.Release();
            }
        }
    }
}
