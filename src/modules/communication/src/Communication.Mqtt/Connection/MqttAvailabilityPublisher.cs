using Communication.Abstractions.Models;
using Communication.Mqtt.Payloads;
using Communication.Mqtt.Publishing;
using Microsoft.Extensions.Logging;

namespace Communication.Mqtt.Connection
{
    public sealed class MqttAvailabilityPublisher(
        MqttPublisher publisher,
        MqttTopicBuilder topics,
        ILogger<MqttAvailabilityPublisher> logger
    )
    {
        public async Task PublishOnlineAsync(DeviceDescriptor device, CancellationToken cancellationToken)
            => await PublishAsync(device, AvailabilityPayloads.OnlinePayload, cancellationToken);
        
        public async Task PublishOfflineAsync(DeviceDescriptor device, CancellationToken cancellationToken)
            => await PublishAsync(device, AvailabilityPayloads.OfflinePayload, cancellationToken);

        private async Task PublishAsync(DeviceDescriptor device, string status, CancellationToken ct)
        {
            await publisher.PublishRetainedAsync(topics.Availability(device), status, ct);
            logger.LogInformation("Published MQTT availability: {Availability}", status);
        }
    }
}
