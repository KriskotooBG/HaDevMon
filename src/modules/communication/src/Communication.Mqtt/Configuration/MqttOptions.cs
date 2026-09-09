namespace Communication.Mqtt.Configuration
{
    public sealed class MqttOptions
    {
        public string Host { get; init; } = string.Empty;

        public int Port { get; init; } = 1883;

        public string? Username { get; init; }

        public string? Password { get; init; }

        public bool UseTls { get; init; }

        public string BaseTopic { get; init; } = "hadevmon";

        public string DiscoveryPrefix { get; init; } = "homeassistant";

        public TimeSpan KeepAlive { get; init; } = TimeSpan.FromSeconds(30);

        public TimeSpan ReconnectInitialDelay { get; init; } = TimeSpan.FromSeconds(1);

        public TimeSpan ReconnectMaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    }
}
