using Installation.Abstractions.ConfigurationWizard.Models;
using Installation.Abstractions.Installation.Enums;
using Installation.Abstractions.Service.Models;

namespace Installation.Abstractions.Installation
{
    public sealed record InstallationRequest(
        InstallationAction Action,
        ServiceConfiguration ServiceConfiguration,
        SetupField[] Fields,
        SetupConfiguration? Configuration
    );
}
