using Installation.Abstractions.Installation;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace Installation.Windows.Security
{
    #pragma warning disable CA1416
    internal static class WindowsElevation
    {
        public static bool IsElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static async Task RelaunchElevatedAsync(InstallationRequest request, CancellationToken cancellationToken)
        {
            var executable = Environment.ProcessPath ?? 
                throw new InvalidOperationException("Unable to determine executable path.");

            var requestPath = Path.Combine(Path.GetTempPath(), $"hadevmon-setup-{Guid.NewGuid():N}.json");
            var resultPath = requestPath + ".result";

            try
            {
                await InstallationRequestFile.WriteAsync(requestPath, request, cancellationToken);

                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                startInfo.ArgumentList.Add("--setup-request");
                startInfo.ArgumentList.Add(requestPath);

                startInfo.ArgumentList.Add("--setup-result");
                startInfo.ArgumentList.Add(resultPath);


                var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to launch elevated installer.");
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0) return;

                var details = File.Exists(resultPath) ? await File.ReadAllTextAsync(resultPath, cancellationToken) : "No error details were returned.";
                throw new InvalidOperationException($"Elevated installer failed with exit code {process.ExitCode}.{Environment.NewLine}{details}");
            }
            catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
            {
                throw new OperationCanceledException("Administrator elevation was cancelled.", exception);
            }
            finally
            {
                TryDelete(requestPath);
                TryDelete(resultPath);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // shrug. we'll get them next time
            }
        }
    }
    #pragma warning restore CA1416
}
