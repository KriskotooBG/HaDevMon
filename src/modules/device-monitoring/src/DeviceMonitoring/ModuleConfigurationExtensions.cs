using DeviceMonitoring.Extensions;
using DeviceMonitoring.Generic.Sensors.Uptime;
using DeviceMonitoring.Windows.Sensors.MemoryUsage;
using DeviceMonitoring.Windows.Commands.Restart;
using DeviceMonitoring.Windows.Commands.Shutdown;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeviceMonitoring.Windows.Sensors.CpuUsage;

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
                .AddSensorIfEnabled<UptimeSensor, UptimeOptions>(sensors.GetRequiredSection("Uptime"))
                .AddSensorIfEnabled<MemoryUsageSensor, MemoryUsageOptions>(sensors.GetRequiredSection("MemoryUsage"))
                .AddSensorIfEnabled<CpuUsageSensor, CpuUsageOptions>(sensors.GetRequiredSection("CpuUsage"));

            services
                .AddCommandIfEnabled<ShutdownCommand, ShutdownOptions>(commands.GetRequiredSection("Shutdown"))
                .AddCommandIfEnabled<RestartCommand, RestartOptions>(commands.GetRequiredSection("Restart"));

            return services;
        }
    }
}
