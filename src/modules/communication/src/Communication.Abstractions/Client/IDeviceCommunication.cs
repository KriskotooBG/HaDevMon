using Communication.Abstractions.Models;

namespace Communication.Abstractions.Client
{
    public interface IDeviceCommunication
    {
        bool IsConnected { get; }

        Task ConnectAsync(DeviceDescriptor device, CancellationToken cancellationToken);

        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
