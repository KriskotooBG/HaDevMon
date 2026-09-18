using Installation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Installation
{
    public static class ModuleConfigurationExtensions
    {
        public static IServiceCollection AddInstallation(this IServiceCollection services)
        {
            if(OperatingSystem.IsWindows())
            { 
                services.AddWindowsInstallation();
                return services;
            }

            if (OperatingSystem.IsLinux())
                throw new NotImplementedException("Linux installation is not implemented yet.");

            throw new PlatformNotSupportedException();
        }
    }
}
