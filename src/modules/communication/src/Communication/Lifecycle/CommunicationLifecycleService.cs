using Communication.Abstractions.Client;
using Communication.Abstractions.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Communication.Lifecycle
{
    internal sealed class CommunicationLifecycleService(
        IDeviceCommunication communication,
        DeviceDescriptor device,
        ILogger<CommunicationLifecycleService> logger
    ) : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            logger.LogInformation("Starting communication for {DeviceName} ({DeviceId})", device.Name, device.Id);
            await communication.ConnectAsync(device, ct);
        }

        public async Task StopAsync(CancellationToken ct)
        {
            logger.LogInformation("Stopping communication");
            await communication.DisconnectAsync(ct);
        }
    }
}
