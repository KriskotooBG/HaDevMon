using DeviceMonitoring.Abstractions.Features;
using DeviceMonitoring.Abstractions.Sensors;

namespace DeviceMonitoring.Generic.Sensors.Uptime
{
    public sealed class UptimeSensor : ISensorContributor
    {
        public string Key => "uptime";

        public string Name => "Uptime";

        public UpdateRate DefaultUpdateRate => UpdateRate.Medium;

        public string? UnitOfMeasurement => "s";


        public ValueTask<SensorReading> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return ValueTask.FromResult(new SensorReading(Math.Floor(uptime.TotalSeconds)));
        }
    }
}
