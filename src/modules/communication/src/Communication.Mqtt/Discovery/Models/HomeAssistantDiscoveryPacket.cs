namespace Communication.Mqtt.Discovery.Models
{
    internal sealed record HomeAssistantDiscoveryPacket
    {
        public required HomeAssistantDevice Device { get; init; }

        public required HomeAssistantOrigin Origin { get; init; }

        public required string AvailabilityTopic { get; init; }

        public string PayloadAvailable { get; init; } = "online";

        public string PayloadNotAvailable { get; init; } = "offline";

        public required Dictionary<string, HomeAssistantComponent> Components { get; init; }
    }
}
