using Installation.Abstractions.ConfigurationWizard.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Installation.Windows.Deployment
{
    internal static class JsonConfigurationWriter
    {
        public static void Apply(string path, IReadOnlyCollection<SetupField> fields, SetupConfiguration configuration)
        {
            var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? [];

            foreach (var field in fields)
            {
                if (!configuration.Values.TryGetValue(field.ConfigurationPath, out var value))
                    continue;

                SetValue(root, field, value);
            }

            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        private static void SetValue(JsonObject root, SetupField field, string? value)
        {
            var segments = field.ConfigurationPath.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                throw new InvalidOperationException($"Invalid configuration path '{field.ConfigurationPath}'.");

            var current = root;
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var segment = segments[index];
                if (current[segment] is JsonObject child)
                {
                    current = child;
                    continue;
                }

                child = [];
                current[segment] = child;
                current = child;
            }

            current[segments[^1]] = ConvertValue(field.Type, value);
        }

        private static JsonValue? ConvertValue(SetupFieldType type, string? value)
        {
            if (value is null) return null;
            return type switch
            {
                SetupFieldType.Integer => JsonValue.Create(long.Parse(value, CultureInfo.InvariantCulture)),
                SetupFieldType.Boolean => JsonValue.Create(bool.Parse(value)),
                _ => JsonValue.Create(value)
            };
        }
    }
}
