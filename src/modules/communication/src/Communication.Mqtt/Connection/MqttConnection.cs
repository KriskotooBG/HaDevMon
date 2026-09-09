using Communication.Abstractions.Models;
using Communication.Mqtt.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Communication.Mqtt.Connection
{
    public sealed class MqttConnection
    {
        private readonly IMqttClient _client;
        private readonly MqttOptions _options;
        private readonly MqttAvailabilityPublisher _availabilityPublisher;
        private readonly MqttClientOptionsFactory _mqttClientOptionsFactory;
        private readonly ILogger<MqttConnection> _logger;


        private readonly SemaphoreSlim _connectGate = new(1, 1);
        private readonly Lock _reconnectSync = new();
        private readonly AsyncReadyStateTracker _ready = new();

        private CancellationTokenSource? _lifetimeCts;
        private Task? _reconnectTask;
        private bool _intentionalDisconnect;



        public DeviceDescriptor? Device { get; private set; }
        public bool IsConnected => _client.IsConnected && _ready.IsSet;



        public MqttConnection(
            IMqttClient client,
            IOptions<MqttOptions> options,
            MqttAvailabilityPublisher availabilityPublisher,
            MqttClientOptionsFactory mqttClientOptionsFactory,
            ILogger<MqttConnection> logger
        ) {
            _client = client;
            _options = options.Value;
            _availabilityPublisher = availabilityPublisher;
            _mqttClientOptionsFactory = mqttClientOptionsFactory;
            _logger = logger;

            _client.DisconnectedAsync += OnDisconnectedAsync;
        }


       
        public async Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(device);

            Device = device;
            _intentionalDisconnect = false;

            _lifetimeCts?.Dispose();
            _lifetimeCts = new CancellationTokenSource();

            try
            {
                await ConnectMqttAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Initial MQTT connection failed. Attempting to reconnect...");
                StartReconnectLoop();
            }
        }


        public Task WaitUntilReadyAsync(CancellationToken cancellationToken)
            => _ready.WaitAsync(cancellationToken);


        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            _intentionalDisconnect = true;

            if (_lifetimeCts is not null)
                await _lifetimeCts.CancelAsync();

            var reconnectTask = _reconnectTask;
            if (reconnectTask is not null)
            {
                try
                {
                    await reconnectTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected while shutting down.
                }
            }

            await _connectGate.WaitAsync(cancellationToken);

            try
            {
                if (!_client.IsConnected)
                {
                    _ready.Reset();
                    return;
                }

                if(Device is not null) await _availabilityPublisher.PublishOfflineAsync(Device, cancellationToken);
                await _client.DisconnectAsync();

                _ready.Reset();
                _logger.LogInformation("Disconnected from MQTT broker.");
            }
            finally
            {
                _connectGate.Release();
            }
        }



        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _ready.Reset();

            if (_intentionalDisconnect)
            {
                _logger.LogDebug("MQTT disconnected intentionally.");
                return Task.CompletedTask;
            }

            _logger.LogWarning(args.Exception, "MQTT connection lost.");

            StartReconnectLoop();
            return Task.CompletedTask;
        }

        private async Task ConnectMqttAsync(CancellationToken cancellationToken)
        {
            var device = Device ?? 
                throw new InvalidOperationException("MQTT device has not been initialized.");

            await _connectGate.WaitAsync(cancellationToken);

            try
            {
                if (!_client.IsConnected)
                {
                    var clientOptions = _mqttClientOptionsFactory.BuildClientOptions(device);

                    _logger.LogInformation("Connecting to MQTT broker {Host}:{Port}", _options.Host, _options.Port);
                    var result = await _client.ConnectAsync(clientOptions, cancellationToken);
                    _logger.LogInformation("Connected to MQTT broker with result code {Result}",result.ResultCode);
                }

                await _availabilityPublisher.PublishOnlineAsync(device, cancellationToken);
                _ready.Set();
            }
            finally
            {
                _connectGate.Release();
            }
        }

        private void StartReconnectLoop()
        {
            lock (_reconnectSync)
            {
                if (_intentionalDisconnect) return;
                if (_lifetimeCts is null || _lifetimeCts.IsCancellationRequested) return;
                if (_reconnectTask is { IsCompleted: false }) return;

                var cancellationToken = _lifetimeCts.Token;

                _reconnectTask = ReconnectLoopAsync(cancellationToken);
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
        {
            var delay = _options.ReconnectInitialDelay;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Reconnecting to MQTT broker in {Delay}...", delay);
                    await Task.Delay(delay, cancellationToken);

                    await ConnectMqttAsync(cancellationToken);

                    _logger.LogInformation("MQTT connection restored.");
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "MQTT reconnection attempt failed.");
                }

                delay = GetNextReconnectDelay(delay);
            }
        }

        private TimeSpan GetNextReconnectDelay(TimeSpan currentDelay)
        {
            var delay = Math.Min(currentDelay.TotalMilliseconds * 2, _options.ReconnectMaxDelay.TotalMilliseconds);
            return TimeSpan.FromMilliseconds(delay);
        }
    }
}
