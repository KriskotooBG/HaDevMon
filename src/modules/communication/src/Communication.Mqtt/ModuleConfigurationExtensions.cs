using Communication.Abstractions.Client;
using Communication.Abstractions.Configuration;
using Communication.Mqtt.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Communication.Mqtt
{
    public static class ModuleConfigurationExtensions
    {
        public static IServiceCollection AddMqttCommunication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<MqttOptions>()
                .Bind(configuration)
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.Host), "MQTT host is required.")
                .Validate(opts => opts.Port is > 0 and <= 65535, "MQTT port must be a valid TCP port.")
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.BaseTopic), "MQTT base topic is required.")
                .Validate(opts => opts.KeepAlive > TimeSpan.Zero, "MQTT keep-alive must be greater than zero.")
                .ValidateOnStart();

            services.AddSingleton<IDeviceCommunication, MqttDeviceCommunication>();

            return services;
        }
    }
}
