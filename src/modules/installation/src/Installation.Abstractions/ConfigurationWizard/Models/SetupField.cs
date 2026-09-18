namespace Installation.Abstractions.ConfigurationWizard.Models
{
    public sealed record SetupField(
        string ConfigurationPath,

        string Label,

        SetupFieldType Type,

        string? DefaultValue = null,

        bool Required = false,

        string? Description = null,

        IReadOnlyList<string>? Options = null
    );
}
