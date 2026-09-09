using Communication.Abstractions.Client;
using Communication.Mqtt.Client;
using Communication.Mqtt.Configuration;
using Communication.Mqtt.Connection;
using Communication.Mqtt.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Communication.Mqtt
{
    public static class ModuleConfigurationExtensions
    {
        public static IServiceCollection AddMqttCommunication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<MqttOptions>()
                .Bind(configuration)
                .WithMqttOptionsValidations()
                .ValidateOnStart();

            services
                .AddSingleton<IMqttClient>(_ => new MqttClientFactory().CreateMqttClient())
                .AddSingleton<MqttConnection>()
                .AddSingleton<MqttPublisher>()
                .AddSingleton<MqttTopicBuilder>()
                .AddSingleton<MqttAvailabilityPublisher>()
                .AddSingleton<MqttClientOptionsFactory>()
                .AddSingleton<IDeviceCommunication, MqttDeviceCommunication>();

            return services;
        }

        private static OptionsBuilder<MqttOptions> WithMqttOptionsValidations(this OptionsBuilder<MqttOptions> optionsBuilder)
        {
            return optionsBuilder
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.Host), "MQTT host is required.")
                .Validate(opts => opts.Port is > 0 and <= 65535, "MQTT port must be a valid TCP port.")
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.BaseTopic), "MQTT base topic is required.")
                .Validate(opts => opts.KeepAlive > TimeSpan.Zero, "MQTT keep-alive must be greater than zero.")
                .Validate(opts => opts.ReconnectInitialDelay > TimeSpan.Zero, "MQTT reconnect initial delay must be greater than zero.")
                .Validate(opts => opts.ReconnectMaxDelay >= opts.ReconnectInitialDelay, "MQTT reconnect max delay must be greater than or equal to the initial delay.");
        }
    }
}
