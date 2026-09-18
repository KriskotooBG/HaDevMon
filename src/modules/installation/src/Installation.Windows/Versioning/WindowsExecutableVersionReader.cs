using System.Diagnostics;

namespace Installation.Windows.Versioning
{
    internal static class WindowsExecutableVersionReader
    {
        public static Version GetVersion(string executablePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

            var info = FileVersionInfo.GetVersionInfo(executablePath);
            var versionText = info.ProductVersion ?? info.FileVersion;

            if (string.IsNullOrWhiteSpace(versionText))
                throw new InvalidOperationException($"Executable '{executablePath}' does not contain version information.");

            return ParseVersion(versionText);
        }

        private static Version ParseVersion(string versionText)
        {
            var numericVersion = versionText.Split(['+', '-'], 2)[0];
            if (!Version.TryParse(numericVersion, out var version))
                throw new InvalidOperationException($"Version '{versionText}' is not a valid version.");

            return version;
        }
    }
}
