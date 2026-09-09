namespace Communication.Mqtt.Discovery.Models
{
    internal sealed record HomeAssistantDevice
    {
        public required string[] Identifiers { get; init; }

        public required string Name { get; init; }

        public string? Manufacturer { get; init; }

        public string? Model { get; init; }
    }
}
