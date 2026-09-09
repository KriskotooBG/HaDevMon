using DeviceMonitoring.Abstractions.Features;
using DeviceMonitoring.Abstractions.Sensors;


namespace DeviceMonitoring.Windows.Sensors.CpuUsage
{
    public sealed class CpuUsageSensor : ISensorContributor
    {
        private CpuTimes? _previous;


        public string Key => "cpu_usage";

        public string Name => "CPU Usage";

        public UpdateRate DefaultUpdateRate => UpdateRate.Fast;

        public string? UnitOfMeasurement => "%";



        public async ValueTask<SensorReading> ReadAsync(CancellationToken cancellationToken)
        {
            var current = WindowsCpuTimes.Read();

            if (_previous is null)
            {
                _previous = current;
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
                current = WindowsCpuTimes.Read();
            }

            var previous = _previous.Value;
            _previous = current;

            var idle = current.Idle - previous.Idle;
            var kernel = current.Kernel - previous.Kernel;
            var user = current.User - previous.User;

            // Windows's kernel time includes idle time. total = kernel + user, busy  = total - idle
            var total = kernel + user;
            if (total == 0) return new SensorReading(0d);

            var busy = total - idle;
            var usage = busy * 100d / total;

            usage = Math.Clamp(usage, 0d, 100d);
            return new SensorReading(Math.Round(usage, 1));
        }
    }
}
