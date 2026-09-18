using Installation.Abstractions.Installation.Enums;

namespace Installation.Utils
{
    internal static class SetupActionResolver
    {
        public static InstallationAction? Resolve(IReadOnlyList<string> args)
        {
            for (var index = 0; index < args.Count; index++)
            {
                var argument = args[index];

                if (argument.StartsWith("--setup-action=", StringComparison.OrdinalIgnoreCase))
                    return Parse(argument["--setup-action=".Length..]);

                if (!argument.Equals("--setup-action", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (index + 1 >= args.Count)
                    throw new ArgumentException("--setup-action requires a value.");

                return Parse(args[index + 1]);
            }

            return null;
        }

        private static InstallationAction Parse(string value)
        {
            if (!Enum.TryParse<InstallationAction>(value, ignoreCase: true, out var action))
                throw new ArgumentException($"Unknown installation action '{value}'.");
            
            return action;
        }
    }
}