using Installation.Abstractions.Service.Models;
using System.Diagnostics;
using System.ServiceProcess;

namespace Installation.Windows.Services
{
    #pragma warning disable CA1416
    internal static class WindowsServiceManager
    {
        public static async Task StopIfRunningAsync(string serviceName, CancellationToken cancellationToken)
        {
            using var service = new ServiceController(serviceName);
            service.Refresh();

            if (service.Status == ServiceControllerStatus.Stopped)
                return;

            if (service.Status != ServiceControllerStatus.StopPending)
                service.Stop();

            await WaitForStatusAsync(service, ServiceControllerStatus.Stopped, cancellationToken);
        }

        public static async Task StartAsync(string serviceName, CancellationToken cancellationToken)
        {
            using var service = new ServiceController(serviceName);
            service.Refresh();

            if (service.Status == ServiceControllerStatus.Running)
                return;

            if (service.Status != ServiceControllerStatus.StartPending)
                service.Start();

            await WaitForStatusAsync(service, ServiceControllerStatus.Running, cancellationToken);
        }

        public static async Task CreateAsync(ServiceConfiguration config, CancellationToken cancellationToken)
        {
            await RunScAsync("create", config.Name, new Dictionary<string, string>
                {
                    { "binPath", config.ExecutableCmd },
                    { "type", "own" },
                    { "start", "auto" },
                    { "obj", "LocalSystem" },
                    { "displayName", config.Name }
                }, cancellationToken
            );

            await RunScAsync("description", config.Name, config.Description, cancellationToken);
        }

        public static async Task ConfigureAsync(ServiceConfiguration config, CancellationToken cancellationToken)
        {
            await RunScAsync("config", config.Name, new Dictionary<string, string>
                {
                    { "binPath", config.ExecutableCmd },
                    { "type", "own" },
                    { "start", "auto" },
                    { "obj", "LocalSystem" },
                    { "displayName", config.Name }
                },
                cancellationToken
            );

            await RunScAsync("description", config.Name, config.Description, cancellationToken);
        }


        private static async Task RunScAsync(string command, string serviceName, string args, CancellationToken cancellationToken)
            => await RunScAsync(command, serviceName, new Dictionary<string, string> { { "", args } }, cancellationToken);

        private static async Task RunScAsync(string command, string serviceName, Dictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "sc.exe"),

                UseShellExecute = false,
                CreateNoWindow = true,

                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            startInfo.ArgumentList.Add(command);
            startInfo.ArgumentList.Add(serviceName);

            foreach (var parameter in parameters)
            {
                if(!string.IsNullOrEmpty(parameter.Key))
                    startInfo.ArgumentList.Add($"{parameter.Key}=");
                startInfo.ArgumentList.Add(parameter.Value);
            }
                
            Console.WriteLine($"Running sc.exe with arguments: {string.Join(" ", startInfo.ArgumentList)}");
            using var process = Process.Start(startInfo) ??
                throw new InvalidOperationException("Failed to start sc.exe.");

            var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode == 0) return;

            throw new InvalidOperationException($"sc.exe failed with exit code {process.ExitCode}.{Environment.NewLine} {await standardOutput} {await standardError}");
        }

        private static async Task WaitForStatusAsync(ServiceController service, ServiceControllerStatus expectedStatus, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(40));

            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                service.Refresh();

                if (service.Status == expectedStatus)
                    return;

                await Task.Delay(TimeSpan.FromMilliseconds(250), timeout.Token);
            }
        }
    }
    #pragma warning restore CA1416
}
