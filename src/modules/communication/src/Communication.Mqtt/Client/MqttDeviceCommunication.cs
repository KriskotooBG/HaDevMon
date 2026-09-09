using Communication.Abstractions.Client;
using Communication.Abstractions.Models;
using Communication.Mqtt.Connection;
using Communication.Mqtt.Publishing;

namespace Communication.Mqtt.Client
{
    public sealed class MqttDeviceCommunication(
        MqttConnection connection,
        MqttPublisher publisher,
        MqttTopicBuilder topics
    ): IDeviceCommunication
    {
        public bool IsConnected => connection.IsConnected;

        

        public Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken)
            => connection.ConnectAsync(device, cancellationToken);

        public Task DisconnectAsync(CancellationToken cancellationToken)
            => connection.DisconnectAsync(cancellationToken);

        public async Task PublishStateAsync(StateUpdate state, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(state);
            var device = connection.Device?? throw new InvalidOperationException("MQTT device has not been initialized.");

            await connection.WaitUntilReadyAsync(cancellationToken);
            await publisher.PublishRetainedAsync(
                topics.State(device, state.Key),
                MqttPayloadFormatter.Format(state.Value),
                cancellationToken
            );
        }
    }
}
