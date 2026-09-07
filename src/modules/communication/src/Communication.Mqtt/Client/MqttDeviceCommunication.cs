using Communication.Abstractions.Client;
using Communication.Abstractions.Configuration;
using Communication.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using System.Globalization;

namespace Communication.Mqtt.Client
{
    public sealed class MqttDeviceCommunication : IDeviceCommunication, IDisposable
    {
        private const string OnlinePayload = "online";
        private const string OfflinePayload = "offline";


        private readonly MqttOptions _options;
        private readonly ILogger<MqttDeviceCommunication> _logger;
        private readonly IMqttClient _client;

        private DeviceDescriptor? _device;

        private string? _availabilityTopic;

        private readonly object _connectionSync = new();
        private TaskCompletionSource<bool> _connectedSignal = CreateConnectionSignal();



        public bool IsConnected => _client.IsConnected;



        public MqttDeviceCommunication(IOptions<MqttOptions> options, ILogger<MqttDeviceCommunication> logger)
        {
            _options = options.Value;
            _logger = logger;

            _client = new MqttClientFactory().CreateMqttClient();

            _client.ConnectedAsync += _ =>
            {
                MarkConnected();

                _logger.LogDebug("MQTT connection marked as ready");
                return Task.CompletedTask;
            };


            _client.DisconnectedAsync += _ =>
            {
                MarkDisconnected();

                _logger.LogWarning("Disconnected from MQTT broker");
                return Task.CompletedTask;
            };
        }

        

        public async Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(device);

            if (_client.IsConnected) return;

            _device = device;
            _availabilityTopic = BuildAvailabilityTopic(device);

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId($"hadevmon-{device.Id}")
                .WithTcpServer(_options.Host,_options.Port)
                .WithKeepAlivePeriod(_options.KeepAlive)
                .WithWillTopic(_availabilityTopic)
                .WithWillPayload(OfflinePayload)
                .WithWillRetain(true)
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                optionsBuilder.WithCredentials(_options.Username,_options.Password ?? string.Empty);

            if (_options.UseTls)
                optionsBuilder.WithTlsOptions(tls => tls.UseTls());


            _logger.LogInformation("Connecting to MQTT broker {Host}:{Port}", _options.Host, _options.Port);
            var result = await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

            _logger.LogInformation("Connected to MQTT broker with result {Result}", result.ResultCode);

            await PublishAvailabilityAsync(OnlinePayload, cancellationToken);
        }

        public async Task PublishStateAsync(StateUpdate state, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (_device is null)
                throw new InvalidOperationException("MQTT device has not been initialized.");

            await WaitUntilConnectedAsync(cancellationToken);

            var topic = BuildStateTopic(_device,state.Key);
            var payload = FormatPayload(state.Value);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await _client.PublishAsync(message, cancellationToken);
        }

        private async Task PublishAvailabilityAsync(string payload, CancellationToken cancellationToken)
        {
            if (_availabilityTopic is null) throw new InvalidOperationException("MQTT availability topic has not been initialized.");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_availabilityTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await _client.PublishAsync(message, cancellationToken);

            _logger.LogInformation("Published MQTT availability: {Availability}", payload);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (!_client.IsConnected) return;

            // A clean MQTT disconnect does not trigger the LWT, so explicitly publish offline first.
            await PublishAvailabilityAsync(OfflinePayload, cancellationToken);
            await _client.DisconnectAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Disconnected cleanly from MQTT broker");
        }

        public void Dispose() => _client.Dispose();

        private static TaskCompletionSource<bool> CreateConnectionSignal()
            => new(TaskCreationOptions.RunContinuationsAsynchronously);

        private void MarkConnected()
        {
            lock (_connectionSync)
            {
                _connectedSignal.TrySetResult(true);
            }
        }

        private void MarkDisconnected()
        {
            lock (_connectionSync)
            {
                if (_connectedSignal.Task.IsCompleted)
                    _connectedSignal = CreateConnectionSignal();
            }
        }

        private Task WaitUntilConnectedAsync(CancellationToken cancellationToken)
        {
            if (_client.IsConnected)
                return Task.CompletedTask;
            
            Task connectionTask;

            lock (_connectionSync)
            {
                if (_client.IsConnected)
                    return Task.CompletedTask;

                connectionTask = _connectedSignal.Task;
            }

            return connectionTask.WaitAsync(cancellationToken);
        }

        private string BuildAvailabilityTopic(DeviceDescriptor device)
        {
            var baseTopic = _options.BaseTopic.Trim().Trim('/');
            return $"{baseTopic}/{device.Id}/availability";
        }

        private string BuildStateTopic(DeviceDescriptor device, string key)
        {
            var baseTopic = _options.BaseTopic.Trim().Trim('/');
            return $"{baseTopic}/{device.Id}/state/{key}";
        }

        private static string FormatPayload(object? value)
        {
            return value switch
            {
                null => string.Empty,
                bool boolean => boolean ? "true" : "false",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,

                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
