using Installation.Abstractions.Application.Enums;

namespace Installation.Utils
{
    internal static class RunModeResolver
    {
        public static ApplicationRunMode Resolve(IReadOnlyCollection<string> args)
        {
            if (args.Any(IsServiceArgument))
                return ApplicationRunMode.Service;

            if (args.Any(IsConsoleArgument))
                return ApplicationRunMode.Console;

            return ApplicationRunMode.Setup;
        }

        private static bool IsServiceArgument(string argument)
        {
            return argument.Equals("--service", StringComparison.OrdinalIgnoreCase) ||
                   argument.Equals("-service", StringComparison.OrdinalIgnoreCase) ||
                   argument.Equals("-S", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsConsoleArgument(string argument)
        {
            return argument.Equals("--console", StringComparison.OrdinalIgnoreCase) ||
                   argument.Equals("-console", StringComparison.OrdinalIgnoreCase) ||
                   argument.Equals("-C", StringComparison.OrdinalIgnoreCase);
        }
    }
}
