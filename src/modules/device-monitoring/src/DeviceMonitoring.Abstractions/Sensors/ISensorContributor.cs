using DeviceMonitoring.Abstractions.Features;

namespace DeviceMonitoring.Abstractions.Sensors
{
    public interface ISensorContributor : IFeatureContributor
    {
        UpdateRate DefaultUpdateRate { get; }

        string? UnitOfMeasurement { get; }

        ValueTask<SensorReading> ReadAsync(CancellationToken cancellationToken);
    }
}
