using Installation.Abstractions.Installation;
using Installation.Abstractions.Service.Models;

namespace Installation.Abstractions
{
    public interface IPlatformInstaller
    {
        Task<InstallationState> GetStateAsync(ServiceConfiguration config, CancellationToken cancellationToken);

        Task ExecuteAsync(InstallationRequest request, CancellationToken cancellationToken);
    }
}
