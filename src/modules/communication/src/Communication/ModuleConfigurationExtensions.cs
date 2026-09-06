using Communication.Abstractions.Models;
using Communication.Configuration;
using Communication.Lifecycle;
using Communication.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Communication
{
    public static class ModuleConfigurationExtensions
    {
        public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);


            var section = configuration
                .GetRequiredSection(CommunicationOptions.SectionName);

            var options = section
                .Get<CommunicationOptions>() ?? throw new InvalidOperationException("Communication configuration is missing.");


            if (options.Provider == CommunicationProvider.None) throw new InvalidOperationException("Communication:Provider must be configured.");
            if (string.IsNullOrWhiteSpace(options.Device.Id)) throw new InvalidOperationException("Communication:Device:Id is required.");
            if (string.IsNullOrWhiteSpace(options.Device.Name)) throw new InvalidOperationException("Communication:Device:Name is required.");

            var device = new DeviceDescriptor(options.Device.Id, options.Device.Name, options.Device.Manufacturer, options.Device.Model);

            services
                .AddSingleton(device)
                .AddHostedService<CommunicationLifecycleService>();

            return options.Provider switch
            {
                CommunicationProvider.Mqtt => services.AddMqttCommunication(section.GetRequiredSection("Mqtt")),

                _ => throw new InvalidOperationException($"Unsupported communication provider '{options.Provider}'.")
            };
        }
    }
}
