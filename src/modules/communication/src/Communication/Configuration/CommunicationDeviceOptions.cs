namespace Communication.Configuration
{
    public sealed class CommunicationDeviceOptions
    {
        public string Id { get; init; } = "ha-devmon";

        public string Name { get; init; } = Environment.MachineName;

        public string? Manufacturer { get; init; }

        public string? Model { get; init; }
    }
}
