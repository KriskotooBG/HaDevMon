using Communication.Abstractions.Entities;
using Communication.Abstractions.Models;
using Communication.Mqtt.Configuration;
using Communication.Mqtt.Discovery.Models;
using Communication.Mqtt.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Communication.Mqtt.Discovery
{
    internal sealed class MqttDiscoveryPublisher(
        MqttPublisher publisher,
        MqttTopicBuilder topics,
        IOptions<MqttOptions> options,
        ILogger<MqttDiscoveryPublisher> logger
    )
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        

        public async Task PublishAsync(DeviceDescriptor device, IReadOnlyCollection<EntityDescriptor> entities, CancellationToken cancellationToken)
        {
            var components = entities.ToDictionary(
                entity => entity.Key,
                entity => CreateComponent(device, entity)
            );

            var packet =
                new HomeAssistantDiscoveryPacket
                {
                    Device = new HomeAssistantDevice
                    {
                        Identifiers = [device.Id],
                        Name = device.Name,
                        Manufacturer = device.Manufacturer,
                        Model = device.Model
                    },

                    Origin = new HomeAssistantOrigin
                    {
                        Name = options.Value.DiscoveryOriginName
                    },

                    AvailabilityTopic = topics.Availability(device),

                    Components = components
                };

            var payload = JsonSerializer.Serialize(packet, SerializerOptions);

            await publisher.PublishRetainedAsync(topics.Discovery(device), payload, cancellationToken);
            logger.LogInformation("Published Home Assistant discovery for {EntityCount} entities", entities.Count);
        }

        private HomeAssistantComponent CreateComponent(DeviceDescriptor device, EntityDescriptor entity)
        {
            return entity.Kind switch
            {
                EntityKind.Sensor => new HomeAssistantComponent
                    {
                        Platform = "sensor",
                        Name = entity.Name,
                        UniqueId = $"{device.Id}_{entity.Key}",
                        StateTopic = topics.State(device, entity.Key),
                        UnitOfMeasurement = entity.UnitOfMeasurement
                    },

                EntityKind.Command => new HomeAssistantComponent
                    {
                        Platform = "button",
                        Name = entity.Name,
                        UniqueId = $"{device.Id}_{entity.Key}",
                        CommandTopic = topics.Command(device, entity.Key),
                        PayloadPress = "PRESS"
                    },

                _ => throw new ArgumentOutOfRangeException(nameof(entity), entity.Kind, "Unsupported entity kind.")
            };
        }
    }
}
