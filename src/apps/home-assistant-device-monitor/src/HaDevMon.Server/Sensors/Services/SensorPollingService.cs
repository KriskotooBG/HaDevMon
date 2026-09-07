using Communication.Abstractions.Client;
using Communication.Abstractions.Models;
using DeviceMonitoring.Abstractions.Sensors;
using HaDevMon.Server.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HaDevMon.Server.Sensors.Services
{
    public sealed class SensorPollingService(
        IEnumerable<SensorRegistration> sensorRegistrations,
        IOptions<PollingOptions> pollingOptions,
        IDeviceCommunication communication,
        ILogger<SensorPollingService> logger
    ) : BackgroundService
    {
        private readonly PollingOptions _pollingOptions = pollingOptions.Value;


        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            SensorRegistration[] sensorRegs = [.. sensorRegistrations];
            logger.LogInformation("Starting sensor polling for {SensorCount} sensor(s)", sensorRegs.Length);

            await Task.WhenAll(sensorRegs.Select(reg => PollSensorAsync(reg, ct)));
        }

        private async Task PollSensorAsync(SensorRegistration registration, CancellationToken ct)
        {
            var sensor = registration.Sensor;
            var interval = registration.UpdateInterval ?? _pollingOptions.GetInterval(sensor.DefaultUpdateRate);

            logger.LogInformation("Polling {SensorName} ({SensorKey}) every {Interval}", sensor.Name, sensor.Key, interval);

            while (!ct.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var reading = await sensor.ReadAsync(ct);
                    logger.LogInformation("Sensor {SensorName} ({SensorKey}) = {Value} {Unit}", sensor.Name, sensor.Key, reading.Value, sensor.UnitOfMeasurement);

                    try
                    {
                        await communication.PublishStateAsync(new StateUpdate(sensor.Key, reading.Value), ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Failed to publish state for {SensorName} ({SensorKey})", sensor.Name, sensor.Key);
                    }

                    stopwatch.Stop();
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to read sensor {SensorName} ({SensorKey})", sensor.Name, sensor.Key);
                }

                try
                {
                    await Task.Delay(interval - stopwatch.Elapsed, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                { 
                    break;
                }
            }
        }
    }
}
