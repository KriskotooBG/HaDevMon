using System.Globalization;

namespace Communication.Mqtt.Publishing
{
    internal static class MqttPayloadFormatter
    {
        public static string Format(object? value)
        {
            return value switch
            {
                null => string.Empty,
                bool boolean => boolean ? "true" : "false",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
