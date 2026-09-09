using Communication.Abstractions.Models;
using Communication.Mqtt.Configuration;
using Microsoft.Extensions.Options;

namespace Communication.Mqtt.Publishing
{
    internal sealed class MqttTopicBuilder(IOptions<MqttOptions> options)
    {
        private readonly string _baseTopic = options.Value.BaseTopic.Trim().Trim('/');
        private readonly string _discoveryPrefix = options.Value.DiscoveryPrefix.Trim().Trim('/');

        public string Availability(DeviceDescriptor device)
            => $"{_baseTopic}/{device.Id}/availability";

        public string State(DeviceDescriptor device, string key)
            => $"{_baseTopic}/{device.Id}/state/{key}";

        public string Command(DeviceDescriptor device, string key)
            => $"{_baseTopic}/{device.Id}/command/{key}";

        public string CommandWildcard(DeviceDescriptor device)
            => $"{_baseTopic}/{device.Id}/command/+";

        public string Discovery(DeviceDescriptor device)
            => $"{_discoveryPrefix}/device/{device.Id}/config";

        public bool TryGetCommandKey(DeviceDescriptor device, string topic, out string key)
        {
            var prefix = $"{_baseTopic}/{device.Id}/command/";
            if (!topic.StartsWith(prefix, StringComparison.Ordinal))
            {
                key = string.Empty;
                return false;
            }

            key = topic[prefix.Length..];
            if (string.IsNullOrWhiteSpace(key) || key.Contains('/'))
            {
                key = string.Empty;
                return false;
            }

            return true;
        }
    }
}
