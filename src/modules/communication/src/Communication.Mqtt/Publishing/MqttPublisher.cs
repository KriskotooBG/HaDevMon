using MQTTnet;
using MQTTnet.Protocol;

namespace Communication.Mqtt.Publishing
{
    public sealed class MqttPublisher(IMqttClient client)
    {
        public async Task PublishRetainedAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(topic);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await client.PublishAsync(message, cancellationToken);
        }
    }
}
