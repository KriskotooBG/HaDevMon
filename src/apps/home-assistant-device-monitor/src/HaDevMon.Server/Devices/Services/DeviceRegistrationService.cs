using Communication.Abstractions.Client;
using Communication.Abstractions.Entities;
using DeviceMonitoring.Abstractions.Commands;
using DeviceMonitoring.Abstractions.Sensors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace HaDevMon.Server.Devices.Services
{
    public sealed class DeviceRegistrationService(
        IEnumerable<SensorRegistration> sensorRegistrations,
        IEnumerable<CommandRegistration> commandRegistrations,
        IDeviceCommunication communication,
        ILogger<DeviceRegistrationService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sensorEntities = sensorRegistrations.Select(registration =>
                new EntityDescriptor(
                    registration.Sensor.Key,
                    registration.Sensor.Name,
                    EntityKind.Sensor,
                    registration.Sensor.UnitOfMeasurement
                )
            );

            var commandEntities = commandRegistrations.Select(registration =>
                new EntityDescriptor(
                    registration.Command.Key,
                    registration.Command.Name,
                    EntityKind.Command
                )
            );

            var entities = sensorEntities
                .Concat(commandEntities)
                .ToArray();

            logger.LogInformation("Registering {EntityCount} device entity(s)", entities.Length);
            await communication.RegisterEntitiesAsync(entities, stoppingToken);
        }
    }
}
