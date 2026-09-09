using DeviceMonitoring.Extensions;
using DeviceMonitoring.Generic.Sensors.Uptime;
using DeviceMonitoring.Windows;
using DeviceMonitoring.Windows.Commands.Restart;
using DeviceMonitoring.Windows.Commands.Shutdown;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceMonitoring
{
    public static class ModuleConfigurationExtensions
    {
        public const string SectionName = "DeviceMonitoring";

        public static IServiceCollection AddDeviceMonitoring(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection(SectionName);
            var sensors = section.GetRequiredSection("Sensors");
            var commands = section.GetRequiredSection("Commands");

            services
                .AddSensorIfEnabled<UptimeSensor, UptimeOptions>(sensors.GetRequiredSection("Uptime"));

            services
                .AddCommandIfEnabled<ShutdownCommand, ShutdownOptions>(commands.GetRequiredSection("Shutdown"))
                .AddCommandIfEnabled<RestartCommand, RestartOptions>(commands.GetRequiredSection("Restart"));

            return services;
        }
    }
}
