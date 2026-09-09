using Communication.Abstractions.Models;
using Communication.Mqtt.Configuration;
using Communication.Mqtt.Payloads;
using Communication.Mqtt.Publishing;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace Communication.Mqtt.Connection
{
    public class MqttClientOptionsFactory(
        IOptions<MqttOptions> options,
        MqttTopicBuilder topics
    )
    {
        private readonly MqttOptions _options = options.Value;


        public MqttClientOptions BuildClientOptions(DeviceDescriptor device)
        {
            var builder = new MqttClientOptionsBuilder()
                .WithClientId($"hadevmon-{device.Id}")
                .WithTcpServer(_options.Host, _options.Port)
                .WithKeepAlivePeriod(_options.KeepAlive)
                .WithWillTopic(topics.Availability(device))
                .WithWillPayload(AvailabilityPayloads.OfflinePayload)
                .WithWillRetain(true)
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                builder.WithCredentials(_options.Username, _options.Password ?? string.Empty);

            if (_options.UseTls)
                builder.WithTlsOptions(tls => tls.UseTls());

            return builder.Build();
        }
    }
}
