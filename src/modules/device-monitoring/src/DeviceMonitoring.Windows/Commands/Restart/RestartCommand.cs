using DeviceMonitoring.Abstractions.Commands;

namespace DeviceMonitoring.Windows.Commands.Restart
{
    public sealed class RestartCommand : ICommandContributor
    {
        public string Key => "restart";

        public string Name => "Restart";


        public ValueTask ExecuteAsync(CancellationToken cancellationToken)
            => new(WindowsPowerController.RestartAsync(cancellationToken));
    }
}
