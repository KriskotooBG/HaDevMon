using Installation.Abstractions.Application.Enums;

namespace Installation.Bootstrap
{

    public sealed record InstallationBootstrapResult(
        ApplicationRunMode Mode,
        bool Handled,
        int ExitCode = 0
    )
    {
        public bool ShouldRunApplication => !Handled;
    }
}
