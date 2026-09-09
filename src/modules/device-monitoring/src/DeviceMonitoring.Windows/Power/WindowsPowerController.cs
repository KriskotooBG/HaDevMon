using System.Diagnostics;

internal sealed class WindowsPowerController
{
    public static Task ShutdownAsync(CancellationToken cancellationToken)
        => ExecuteAsync(["/s", "/t", "0"], cancellationToken);

    public static Task RestartAsync(CancellationToken cancellationToken)
        => ExecuteAsync(["/r", "/t", "0"], cancellationToken);

    private static async Task ExecuteAsync(IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        var sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var executable = Path.Combine(sysFolder, "shutdown.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Windows power command.");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0) throw new InvalidOperationException($"Windows power command exited with code {process.ExitCode}.");
    }
}
