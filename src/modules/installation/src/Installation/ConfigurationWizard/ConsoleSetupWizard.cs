using Installation.Abstractions.ConfigurationWizard.Models;
using System.Globalization;
using System.Text;

namespace Installation.ConfigurationWizard
{
    internal static class ConsoleSetupWizard
    {
        public static SetupConfiguration Run(IReadOnlyCollection<SetupField> fields)
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            Console.WriteLine();
            Console.WriteLine("Configuration");
            Console.WriteLine("------------------");
            Console.WriteLine();

            foreach (var field in fields)
                values[field.ConfigurationPath] = Prompt(field);

            return new SetupConfiguration(values);
        }

        private static string? Prompt(SetupField field)
        {
            while (true)
            {
                Console.WriteLine(field.Label);
                if (!string.IsNullOrWhiteSpace(field.Description)) Console.WriteLine(field.Description);

                if (!string.IsNullOrWhiteSpace(field.DefaultValue)) Console.Write($"[{field.DefaultValue}]: ");
                else Console.Write("> ");

                var value = ReadValue(field);

                if (string.IsNullOrWhiteSpace(value))
                    value = field.DefaultValue;

                if (field.Required && string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("A value is required.");
                    Console.WriteLine();
                    continue;
                }

                Console.WriteLine();
                return value;
            }
        }

        private static string? ReadValue(SetupField field)
        {
            return field.Type switch
            {
                SetupFieldType.Text => Console.ReadLine(),
                SetupFieldType.Integer => ReadInteger(),
                SetupFieldType.Boolean => ReadBoolean(),
                SetupFieldType.Selection => ReadSelection(field),
                SetupFieldType.Secret => ReadSecret(),

                _ => throw new ArgumentOutOfRangeException(nameof(field))
            };
        }

        private static string? ReadInteger()
        {
            while (true)
            {
                var value = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(value)) return null;

                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                    return number.ToString(CultureInfo.InvariantCulture);

                Console.Write("Enter a valid integer: ");
            }
        }

        private static string? ReadBoolean()
        {
            while (true)
            {
                var value = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(value)) return null;

                switch (value.Trim().ToLowerInvariant())
                {
                    case "y":
                    case "yes":
                    case "true":
                    case "1":
                        return "true";

                    case "n":
                    case "no":
                    case "false":
                    case "0":
                        return "false";
                }

                Console.Write("Enter yes or no: ");
            }
        }

        private static string? ReadSelection(SetupField field)
        {
            if (field.Options is null || field.Options.Count == 0)
                throw new InvalidOperationException($"Selection field '{field.ConfigurationPath}' has no options.");

            Console.WriteLine();

            for (var index = 0; index < field.Options.Count; index++)
                Console.WriteLine($"  [{index + 1}] {field.Options[index]}");

            Console.Write("> ");

            while (true)
            {
                var value = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(value)) return null;

                if (int.TryParse(value, out var selectedIndex) && selectedIndex >= 1 && selectedIndex <= field.Options.Count)
                    return field.Options[selectedIndex - 1];

                var option = field.Options.FirstOrDefault(candidate => candidate.Equals(value, StringComparison.OrdinalIgnoreCase));
                if (option is not null) return option;

                Console.Write("Select one of the listed options: ");
            }
        }

        private static string? ReadSecret()
        {
            var value = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return value.Length == 0 ? null : value.ToString();
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (value.Length == 0) continue;

                    value.Length--;
                    Console.Write("\b \b");
                    continue;
                }

                if (char.IsControl(key.KeyChar)) continue;

                value.Append(key.KeyChar);
                Console.Write('*');
            }
        }
    }
}
