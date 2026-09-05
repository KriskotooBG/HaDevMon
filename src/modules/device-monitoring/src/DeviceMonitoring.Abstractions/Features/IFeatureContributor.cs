namespace DeviceMonitoring.Abstractions.Features
{
    public interface IFeatureContributor
    {
        string Key { get; }

        string Name { get; }
    }
}
