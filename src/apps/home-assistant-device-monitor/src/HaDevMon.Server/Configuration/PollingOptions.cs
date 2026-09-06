using DeviceMonitoring.Abstractions.Features;

namespace HaDevMon.Server.Configuration
{
    public sealed class PollingOptions
    {
        public const string SectionName = "Polling";


        public TimeSpan Fast { get; init; } = TimeSpan.FromSeconds(5);

        public TimeSpan Medium { get; init; } = TimeSpan.FromSeconds(15);

        public TimeSpan Slow { get; init; } = TimeSpan.FromMinutes(1);


        public TimeSpan GetInterval(UpdateRate updateRate) =>
            updateRate switch
            {
                UpdateRate.Fast => Fast,
                UpdateRate.Medium => Medium,
                UpdateRate.Slow => Slow,
                _ => throw new ArgumentOutOfRangeException(nameof(updateRate), updateRate, null)
            };
    }
}
