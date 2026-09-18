using Installation.Abstractions.Installation;
using Installation.Abstractions.Service.Models;

namespace Installation.Windows.Deployment
{
    internal static class WindowsApplicationDeployment
    {
        public static void Deploy(InstallationRequest request)
        {
            var candidateExecutable = Environment.ProcessPath ??
                throw new InvalidOperationException("Unable to determine candidate executable path.");

            Directory.CreateDirectory(request.ServiceConfiguration.InstallationPath);
            DeployExecutable(request.ServiceConfiguration, candidateExecutable);

            DeployConfigurationIfMissing(request.ServiceConfiguration);

            if (request.Configuration is not null)
                JsonConfigurationWriter.Apply(request.ServiceConfiguration.ConfigurationPath, request.Fields, request.Configuration);
        }

        private static void DeployExecutable(ServiceConfiguration config, string candidateExecutable)
        {
            if (Path.GetFullPath(candidateExecutable).Equals(Path.GetFullPath(config.ExecutablePath), StringComparison.OrdinalIgnoreCase))
                return;

            var temporaryPath = config.InstallationPath + ".new";

            File.Copy(candidateExecutable, temporaryPath, overwrite: true);
            File.Move(temporaryPath, config.ExecutablePath, overwrite: true);
        }

        private static void DeployConfigurationIfMissing(ServiceConfiguration config)
        {

            if (File.Exists(config.ConfigurationPath)) return;

            var source = Path.Combine(AppContext.BaseDirectory, config.ConfigurationFilename);
            if (!File.Exists(source))
                throw new FileNotFoundException($"{config.ConfigurationFilename} was not found beside the candidate executable.", source);

            File.Copy(source, config.ConfigurationPath);
        }
    }
}
