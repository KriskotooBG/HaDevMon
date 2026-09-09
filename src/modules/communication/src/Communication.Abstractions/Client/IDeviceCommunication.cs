using Communication.Abstractions.Commands;
using Communication.Abstractions.Entities;
using Communication.Abstractions.Models;

namespace Communication.Abstractions.Client
{
    public interface IDeviceCommunication
    {
        bool IsConnected { get; }

        Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken);

        Task RegisterEntitiesAsync(IReadOnlyCollection<EntityDescriptor> entities, CancellationToken cancellationToken);

        IAsyncEnumerable<CommandInvocation> ReadCommandsAsync(CancellationToken cancellationToken);

        Task PublishStateAsync(StateUpdate state, CancellationToken cancellationToken);

        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
