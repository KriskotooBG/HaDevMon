using DeviceMonitoring.Abstractions.Commands;

namespace DeviceMonitoring.Windows.Commands.Shutdown
{
    public sealed class ShutdownCommand : ICommandContributor
    {
        public string Key => "shutdown";
        public string Name => "Shutdown";


        public ValueTask ExecuteAsync(CancellationToken cancellationToken)
         => new(WindowsPowerController.ShutdownAsync(cancellationToken));
    }
}
