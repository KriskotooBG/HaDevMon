using Communication.Abstractions.Commands;
using Communication.Mqtt.Connection;
using Communication.Mqtt.Publishing;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System.Threading.Channels;

namespace Communication.Mqtt.Commands
{
    internal sealed class MqttCommandReceiver
    {
        private const string PressPayload = "PRESS";

        private readonly IMqttClient _client;
        private readonly MqttConnection _connection;
        private readonly MqttTopicBuilder _topics;
        private readonly ILogger<MqttCommandReceiver> _logger;

        private readonly Channel<CommandInvocation> _commands = Channel.CreateUnbounded<CommandInvocation>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }
        );



        public MqttCommandReceiver(
            IMqttClient client,
            MqttConnection connection,
            MqttTopicBuilder topics,
            ILogger<MqttCommandReceiver> logger
        )
        {
            _client = client;
            _connection = connection;
            _topics = topics;
            _logger = logger;

            _connection.ConnectionReadyAsync += OnConnectionReadyAsync;
            _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        }



        public IAsyncEnumerable<CommandInvocation>ReadAllAsync(CancellationToken cancellationToken)
            => _commands.Reader.ReadAllAsync(cancellationToken);


        private async Task OnConnectionReadyAsync(CancellationToken cancellationToken)
        {
            var device = _connection.Device;
            if (device is null) return;

            var topic = _topics.CommandWildcard(device);

            var subscribeOptions = new MqttClientFactory()
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter
                    .WithTopic(topic)
                    .WithAtMostOnceQoS()
                )
                .Build();

            await _client.SubscribeAsync(subscribeOptions, cancellationToken);
            _logger.LogInformation("Subscribed to MQTT command topic {Topic}", topic);
        }

        private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var device = _connection.Device;
            if (device is null) return Task.CompletedTask;
            if (!_topics.TryGetCommandKey(device, args.ApplicationMessage.Topic, out var key))
                return Task.CompletedTask;

            // Don't execute retained command messages...
            if (args.ApplicationMessage.Retain)
            {
                _logger.LogWarning("Ignoring retained MQTT command {CommandKey}", key);
                return Task.CompletedTask;
            }

            var payload = args.ApplicationMessage.ConvertPayloadToString();
            if (!string.Equals(payload, PressPayload, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Ignoring MQTT command {CommandKey} with unexpected payload '{Payload}'", key, payload);
                return Task.CompletedTask;
            }

            if (!_commands.Writer.TryWrite(new CommandInvocation(key)))
                _logger.LogWarning("Failed to queue MQTT command {CommandKey}", key);
            
            return Task.CompletedTask;
        }
    }
}
