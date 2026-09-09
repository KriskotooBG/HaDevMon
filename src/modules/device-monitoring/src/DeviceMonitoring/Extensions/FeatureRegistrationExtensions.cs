using DeviceMonitoring.Abstractions.Commands;
using DeviceMonitoring.Abstractions.Configuration;
using DeviceMonitoring.Abstractions.Sensors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DeviceMonitoring.Extensions
{
    internal static class FeatureRegistrationExtensions
    {
        internal static IServiceCollection AddSensorIfEnabled<TImplementation, TOptions>(
            this IServiceCollection services,
            IConfigurationSection section
        ) where TImplementation : class, ISensorContributor where TOptions : SensorFeatureOptions
        {
            var options = section.Get<TOptions>();
            if (options is null || !options.Enabled)
                return services;

            services
                .AddOptions<TOptions>()
                .Bind(section);

            services
                .AddSingleton<TImplementation>()
                .AddSingleton<ISensorContributor>(sp => sp.GetRequiredService<TImplementation>())
                .AddSingleton(sp =>
                {
                    var sensor = sp.GetRequiredService<TImplementation>();
                    var featureOptions = sp.GetRequiredService<IOptions<TOptions>>().Value;
                
                    return new SensorRegistration(sensor, featureOptions.UpdateInterval);
                });

            return services;
        }

        internal static IServiceCollection AddCommandIfEnabled<TImplementation, TOptions>(
            this IServiceCollection services,
            IConfigurationSection section
        ) where TImplementation : class, ICommandContributor where TOptions : CommandFeatureOptions
        {
            var options = section.Get<TOptions>();

            if (options is null || !options.Enabled)
                return services;
        

            services
                .AddOptions<TOptions>()
                .Bind(section);

            services
                .AddSingleton<TImplementation>()
                .AddSingleton(sp => new CommandRegistration(sp.GetRequiredService<TImplementation>()));

            return services;
        }
    }
}
