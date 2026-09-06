using DeviceMonitoring.Extensions;
using DeviceMonitoring.Generic.Sensors.Uptime;
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


            services
                .AddSensorIfEnabled<UptimeSensor, UptimeOptions>(sensors.GetRequiredSection("Uptime"));

            return services;
        }
    }
}
