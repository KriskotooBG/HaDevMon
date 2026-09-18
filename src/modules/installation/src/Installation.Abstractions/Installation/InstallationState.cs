using Installation.Abstractions.Installation.Enums;

namespace Installation.Abstractions.Installation
{
    public sealed record InstallationState(
        bool IsInstalled,
        Version CandidateVersion,
        Version? InstalledVersion,
        string? InstalledExecutablePath
    )
    {
        public InstallationAction GetReplacementAction()
        {
            if (!IsInstalled)
                return InstallationAction.Install;

            if (InstalledVersion is null)
                return InstallationAction.Reinstall;

            return CandidateVersion.CompareTo(InstalledVersion) switch
            {
                > 0 => InstallationAction.Update,
                < 0 => InstallationAction.Downgrade,
                _ => InstallationAction.Reinstall
            };
        }
    }
}
