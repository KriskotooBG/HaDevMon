namespace DeviceMonitoring.Abstractions.Sensors
{
    public sealed record SensorRegistration(ISensorContributor Sensor, TimeSpan? UpdateInterval);
}
