using Installation.Abstractions;
using Installation.Abstractions.Application.Enums;
using Installation.Abstractions.ConfigurationWizard.Models;
using Installation.Abstractions.Installation;
using Installation.Abstractions.Installation.Enums;
using Installation.Abstractions.Service.Models;
using Installation.ConfigurationWizard;
using Installation.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Installation.Bootstrap
{
    public static class InstallationBootstrap
    {
        public static async Task<InstallationBootstrapResult> RunAsync(
            string[] args,
            ServiceConfiguration serviceConfiguration,
            IReadOnlyCollection<SetupField> setupFields,
            CancellationToken cancellationToken = default
        )
        {
            var mode = RunModeResolver.Resolve(args);
            if (mode is ApplicationRunMode.Service or ApplicationRunMode.Console)
                return new InstallationBootstrapResult(mode, false);
            
            var exitCode = await RunSetupAsync(serviceConfiguration, setupFields, args, cancellationToken);
            return new InstallationBootstrapResult(ApplicationRunMode.Setup, true, exitCode);
        }

        private static async Task<int> RunSetupAsync(ServiceConfiguration serviceConfiguration, IReadOnlyCollection<SetupField> setupFields, string[] args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine();
            Console.WriteLine("<*========+========*>");
            Console.WriteLine("Setup");
            Console.WriteLine();

            var services = new ServiceCollection();
            services.AddInstallation();

            await using var serviceProvider = services.BuildServiceProvider();
            var installer = serviceProvider.GetRequiredService<IPlatformInstaller>();

            var requestPath = GetArgumentValue(args, "--setup-request");
            var resultPath = GetArgumentValue(args, "--setup-result");

            if (requestPath is not null)
            {
                try
                {
                    var request = await InstallationRequestFile.ReadAsync(requestPath, cancellationToken);
                    await installer.ExecuteAsync(request, cancellationToken);

                    return 0;
                }
                catch (Exception exception)
                {
                    if (resultPath is not null)
                        await File.WriteAllTextAsync(resultPath, exception.ToString(), CancellationToken.None);

                    return 1;
                }
            }

            var requestedAction = SetupActionResolver.Resolve(args);
            var state = await installer.GetStateAsync(serviceConfiguration, cancellationToken);

            InstallationAction action;

            if (requestedAction is not null)
                action = requestedAction.Value;
            else
            {
                PrintState(state);

                var selection = PromptForAction(state);
                if (selection is null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Cancelled.");

                    return 0;
                }

                action = selection.Value;

                if (!ConfirmAction(action, state))
                {
                    Console.WriteLine();
                    Console.WriteLine("Cancelled.");

                    return 0;
                }
            }

            try
            {
                Console.WriteLine();
                Console.WriteLine($"{GetProgressVerb(action)} {serviceConfiguration.Name}...");

                SetupConfiguration? configuration = null;

                if (action == InstallationAction.Install)
                    configuration = ConsoleSetupWizard.Run(setupFields);

                var request = new InstallationRequest(action, serviceConfiguration, [..setupFields], configuration);
                await installer.ExecuteAsync(request, cancellationToken);

                Console.WriteLine();
                Console.WriteLine($"{serviceConfiguration.Name} service is installed and running.");

                return 0;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Console.WriteLine("Operation cancelled.");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"Installation failed: {exception.Message}");
            }

            return 1;
        }

        private static void PrintState(InstallationState state)
        {
            Console.WriteLine($"This version: {state.CandidateVersion}");

            if (!state.IsInstalled)
            {
                Console.WriteLine("The application is not installed yet. What would you like to?");
                Console.WriteLine();

                Console.WriteLine("[I] Yes - Install");
                Console.WriteLine("[C] Cancel");

                return;
            }

            Console.WriteLine($"Installed version: {state.InstalledVersion?.ToString() ?? "unknown"}");
            Console.WriteLine();

            string? msg = state.GetReplacementAction() switch
            {
                InstallationAction.Update => $"[U] Update to {state.CandidateVersion}",
                InstallationAction.Reinstall => $"[R] Reinstall {state.CandidateVersion}",
                InstallationAction.Downgrade => $"[D] Downgrade to {state.CandidateVersion}",
                _ => null
            };
            if (msg is not null) Console.WriteLine(msg);

            Console.WriteLine("[X] Uninstall");
            Console.WriteLine("[C] Cancel");
        }

        private static InstallationAction? PromptForAction(InstallationState state)
        {
            var replacement = state.GetReplacementAction();

            var expectedKey = replacement switch
                {
                    InstallationAction.Install => "I",
                    InstallationAction.Update => "U",
                    InstallationAction.Reinstall => "R",
                    InstallationAction.Downgrade => "D",
                    _ => throw new ArgumentOutOfRangeException()
                };

            while (true)
            {
                Console.WriteLine();
                Console.Write($"[{expectedKey}] {replacement}  [C] Cancel: ");

                var input = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (input == "C") return null;
                if (input == expectedKey) return replacement;
            }
        }

        private static bool ConfirmAction(InstallationAction action, InstallationState state)
        {
            Console.WriteLine();

            var text = action switch
            {
                InstallationAction.Install => $"Install v{state.CandidateVersion}?",
                InstallationAction.Update => $"Update v{state.InstalledVersion} -> v{state.CandidateVersion}? ",
                InstallationAction.Reinstall => $"Reinstall v{state.CandidateVersion}? ",
                InstallationAction.Downgrade => $"Downgrade v{state.InstalledVersion} → v{state.CandidateVersion}? ",
                _ => throw new ArgumentOutOfRangeException(nameof(action))
            };

            Console.Write($"{text} [y/N]: ");

            return string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
        }
        private static string GetProgressVerb(InstallationAction action)
        {
            return action switch
            {
                InstallationAction.Install => "Installing",
                InstallationAction.Update => "Updating",
                InstallationAction.Reinstall => "Reinstalling",
                InstallationAction.Downgrade => "Downgrading",
                _ => action.ToString()
            };
        }

        private static string? GetArgumentValue(IReadOnlyList<string> args, string name)
        {
            for (var index = 0; index < args.Count; index++)
            {
                if (!args[index].Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
                if (index + 1 >= args.Count) throw new ArgumentException($"{name} requires a value.");

                return args[index + 1];
            }

            return null;
        }
    }
}
