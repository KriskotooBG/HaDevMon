namespace Communication.Mqtt.Discovery.Models
{
    internal sealed record HomeAssistantComponent
    {
        public required string Platform { get; init; }

        public required string Name { get; init; }

        public required string UniqueId { get; init; }

        public string? StateTopic { get; init; }

        public string? CommandTopic { get; init; }

        public string? PayloadPress { get; init; }

        public string? UnitOfMeasurement { get; init; }
    }
}
