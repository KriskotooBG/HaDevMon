using Installation.Abstractions;
using Installation.Abstractions.Installation;
using Installation.Abstractions.Installation.Enums;
using Installation.Abstractions.Service.Models;
using Installation.Windows.Deployment;
using Installation.Windows.Security;
using Installation.Windows.Services;
using Installation.Windows.Versioning;
using System.ServiceProcess;

namespace Installation.Windows.Installer
{
    public sealed class WindowsPlatformInstaller : IPlatformInstaller
    {
        public Task<InstallationState> GetStateAsync(ServiceConfiguration config, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var candidatePath = Environment.ProcessPath 
                ?? throw new InvalidOperationException("Unable to determine the current executable path.");

            var candidateVersion = WindowsExecutableVersionReader.GetVersion(candidatePath);
            var isInstalled = ServiceExists(config.Name);
            Version? installedVersion = null;

            if (isInstalled && File.Exists(config.InstallationPath))
                installedVersion = WindowsExecutableVersionReader.GetVersion(config.InstallationPath);

            return Task.FromResult(
                new InstallationState(
                    isInstalled,
                    candidateVersion,
                    installedVersion,
                    isInstalled ? config.InstallationPath : null
                )
            );
        }

        public async Task ExecuteAsync(InstallationRequest request, CancellationToken cancellationToken)
        {
            if (!WindowsElevation.IsElevated())
            {
                await WindowsElevation.RelaunchElevatedAsync(request, cancellationToken);
                return;
            }

            var state = await GetStateAsync(request.ServiceConfiguration, cancellationToken);
            var expectedAction = state.GetReplacementAction();
            
            if (request.Action != expectedAction)
                throw new InvalidOperationException($"Requested action '{request.Action}' does not match the current installation state. Expected '{expectedAction}'.");

            if (state.IsInstalled)
                await WindowsServiceManager.StopIfRunningAsync(request.ServiceConfiguration.Name, cancellationToken);

            WindowsApplicationDeployment.Deploy(request);

            if (state.IsInstalled)
                await WindowsServiceManager.ConfigureAsync(request.ServiceConfiguration, cancellationToken);
            else
                await WindowsServiceManager.CreateAsync(request.ServiceConfiguration, cancellationToken);

            await WindowsServiceManager.StartAsync(request.ServiceConfiguration.Name, cancellationToken);
        }

        private static bool ServiceExists(string serviceName)
        {
            #pragma warning disable CA1416
            return ServiceController
                .GetServices()
                .Any(service => service.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            #pragma warning restore CA1416
        }
    }
}
