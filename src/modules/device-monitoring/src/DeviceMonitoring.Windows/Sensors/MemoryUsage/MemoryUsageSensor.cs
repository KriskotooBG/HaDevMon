using DeviceMonitoring.Abstractions.Features;
using DeviceMonitoring.Abstractions.Sensors;

namespace DeviceMonitoring.Windows.Sensors.MemoryUsage
{
    public sealed class MemoryUsageSensor : ISensorContributor
    {
        public string Key => "memory_usage";

        public string Name => "Memory Usage";

        public UpdateRate DefaultUpdateRate => UpdateRate.Medium;

        public string? UnitOfMeasurement => "%";



        public ValueTask<SensorReading> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var memory = WindowsMemoryStatus.Read();
            if (memory.TotalPhysical == 0)
                return ValueTask.FromResult(new SensorReading(0d));

            var usage = memory.UsedPhysical * 100d / memory.TotalPhysical;
            return ValueTask.FromResult(new SensorReading(Math.Round(usage, 1)));
        }
    }
}
