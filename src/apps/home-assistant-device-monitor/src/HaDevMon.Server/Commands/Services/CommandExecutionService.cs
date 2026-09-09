using Communication.Abstractions.Client;
using DeviceMonitoring.Abstractions.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace HaDevMon.Server.Commands.Services
{
    public sealed class CommandExecutionService(
        IEnumerable<CommandRegistration> commandRegistrations,
        IDeviceCommunication communication,
        ILogger<CommandExecutionService> logger
    ) : BackgroundService
    {
        private readonly Dictionary<string, ICommandContributor> commands = commandRegistrations
            .Select(registration => registration.Command)
            .ToDictionary(command => command.Key, StringComparer.Ordinal);


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Listening for {CommandCount} command(s)", commands.Count);

            await foreach (var invocation in communication.ReadCommandsAsync(stoppingToken))
            {
                if (!commands.TryGetValue(invocation.Key, out var command))
                {
                    logger.LogWarning("Received unknown command {CommandKey}", invocation.Key);
                    continue;
                }

                try
                {
                    logger.LogInformation("Executing command {CommandName} ({CommandKey})", command.Name, command.Key);
                    await command.ExecuteAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,"Command {CommandName} ({CommandKey}) failed", command.Name, command.Key);
                }
            }
        }
    }
}
