namespace Communication.Mqtt.Discovery.Models
{
    internal sealed record HomeAssistantOrigin
    {
        public required string Name { get; init; }
    }
}
