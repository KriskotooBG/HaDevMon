using Communication.Abstractions.Models;
using Communication.Mqtt.Configuration;
using Microsoft.Extensions.Options;

namespace Communication.Mqtt.Publishing
{
    public sealed class MqttTopicBuilder(IOptions<MqttOptions> options)
    {
        private readonly string _baseTopic = options.Value.BaseTopic.Trim().Trim('/');

        public string Availability(DeviceDescriptor device)
            => $"{_baseTopic}/{device.Id}/availability";

        public string State(DeviceDescriptor device, string key)
            => $"{_baseTopic}/{device.Id}/state/{key}";
    }
}
