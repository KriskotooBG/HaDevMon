using Installation.Abstractions;
using Installation.Windows.Installer;
using Microsoft.Extensions.DependencyInjection;

namespace Installation.Windows
{
    public static class ModuleConfigurationExtensions
    {
        public static IServiceCollection AddWindowsInstallation(this IServiceCollection services)
        {
            services
                .AddSingleton<IPlatformInstaller, WindowsPlatformInstaller>();

            return services;
        }
    }
}
