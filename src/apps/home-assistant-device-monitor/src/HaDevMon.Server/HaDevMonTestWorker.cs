using DeviceMonitoring.Abstractions.Sensors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace HaDevMon.Server
{
    public sealed class HaDevMonTestWorker(
        ILogger<HaDevMonTestWorker> logger,
        IEnumerable<ISensorContributor> sensors
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var registeredSensors = sensors.ToArray();

            logger.LogInformation("HaDevMon is running with {SensorCount} sensor(s)", registeredSensors.Length);

            foreach (var sensor in registeredSensors)
            {
                var reading = await sensor.ReadAsync(ct);

                logger.LogInformation(
                    "Sensor {SensorName} ({SensorKey}) = {Value} {Unit}",
                    sensor.Name,
                    sensor.Key,
                    reading.Value,
                    sensor.UnitOfMeasurement);
            }

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Expected during shutdown.
            }

            logger.LogInformation("HaDevMon is stopping.");
        }
    }
}
