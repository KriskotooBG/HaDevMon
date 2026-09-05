using DeviceMonitoring.Abstractions.Features;

namespace DeviceMonitoring.Abstractions.Commands
{
    public interface ICommandContributor : IFeatureContributor
    {
        ValueTask ExecuteAsync(CancellationToken cancellationToken);
    }
}
