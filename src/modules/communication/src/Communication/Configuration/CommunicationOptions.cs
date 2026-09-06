namespace Communication.Configuration
{
    public sealed class CommunicationOptions
    {
        public const string SectionName = "Communication";

        public CommunicationProvider Provider { get; init; }

        public CommunicationDeviceOptions Device { get; init; } = new();
    }
}
