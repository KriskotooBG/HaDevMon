namespace DeviceMonitoring.Abstractions.Configuration
{
    public class SensorFeatureOptions : FeatureOptions
    {
        public TimeSpan? UpdateInterval { get; init; }
    }
}
