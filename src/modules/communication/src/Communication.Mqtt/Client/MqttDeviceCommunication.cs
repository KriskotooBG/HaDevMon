using Communication.Abstractions.Client;
using Communication.Abstractions.Commands;
using Communication.Abstractions.Entities;
using Communication.Abstractions.Models;
using Communication.Mqtt.Commands;
using Communication.Mqtt.Connection;
using Communication.Mqtt.Discovery;
using Communication.Mqtt.Publishing;

namespace Communication.Mqtt.Client
{
    internal sealed class MqttDeviceCommunication(
        MqttConnection connection,
        MqttPublisher publisher,
        MqttTopicBuilder topics,
        HomeAssistantDiscoveryCoordinator discoveryCoordinator,
        MqttCommandReceiver commandReceiver
    ) : IDeviceCommunication
    {
        public bool IsConnected => connection.IsConnected;

        

        public Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken)
            => connection.ConnectAsync(device, cancellationToken);

        public Task DisconnectAsync(CancellationToken cancellationToken)
            => connection.DisconnectAsync(cancellationToken);

        public Task RegisterEntitiesAsync(IReadOnlyCollection<EntityDescriptor> entities, CancellationToken cancellationToken)
            => discoveryCoordinator.RegisterEntitiesAsync(entities, cancellationToken);

        public IAsyncEnumerable<CommandInvocation>ReadCommandsAsync(CancellationToken cancellationToken)
            => commandReceiver.ReadAllAsync(cancellationToken);

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
