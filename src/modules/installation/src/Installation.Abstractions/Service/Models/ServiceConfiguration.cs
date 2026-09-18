namespace Installation.Abstractions.Service.Models
{
    public sealed record ServiceConfiguration(
        string Name,
        string Description,
        string InstallationPath,
        string ExecutableName,
        string Arguments,
        string ConfigurationFilename = "appsettings.json"
    )
    {
        public string ExecutablePath => Path.Combine(InstallationPath, ExecutableName);
        public string ExecutableCmd => $"\"{ExecutablePath}\" {Arguments}";

        public string ConfigurationPath => Path.Combine(InstallationPath, ConfigurationFilename);
    }
}
