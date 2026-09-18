using Installation.Abstractions.ConfigurationWizard.Models;

namespace HaDevMon.Server.SetupWizard
{
    internal static class SetupConfigurationDefinition
    {
        public static IReadOnlyCollection<SetupField> Fields =>
        [
            new(
                "Communication:Device:Id",
                "Device ID",
                SetupFieldType.Text,
                DefaultValue: Environment.MachineName.ToLowerInvariant(),
                Required: true,
                Description: "Stable identifier used for MQTT and Home Assistant."
            ),

            new(
                "Communication:Device:Name",
                "Device name",
                SetupFieldType.Text,
                DefaultValue: Environment.MachineName,
                Required: true
            ),

            new(
                "Communication:Mqtt:Host",
                "MQTT broker address",
                SetupFieldType.Text,
                Required: true
            ),

            new(
                "Communication:Mqtt:Port",
                "MQTT broker port",
                SetupFieldType.Integer,
                DefaultValue: "1883",
                Required: true
            ),

            new(
                "Communication:Mqtt:Username",
                "MQTT username",
                SetupFieldType.Text
            ),

            new(
                "Communication:Mqtt:Password",
                "MQTT password",
                SetupFieldType.Secret
            )
        ];
    }
}
