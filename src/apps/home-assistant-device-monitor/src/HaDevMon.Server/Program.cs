using Communication;
using DeviceMonitoring;
using HaDevMon.Server.Commands.Services;
using HaDevMon.Server.Configuration;
using HaDevMon.Server.Devices.Services;
using HaDevMon.Server.Logging;
using HaDevMon.Server.Sensors.Services;
using HaDevMon.Server.SetupWizard;
using Hosting.Windows;
using Installation.Abstractions.Service.Models;
using Installation.Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var executableName = "HaDevMon.Server.exe";
var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
var installPath = Path.Combine(programFiles, "HaDevMon.Server");

var appConfig = new ServiceConfiguration(
    Name: "HaDevMon",
    Description: "Home Assistant Device Monitor",
    InstallationPath: installPath,
    ExecutableName: executableName,
    Arguments: "--service"
);

var installation =  await InstallationBootstrap.RunAsync(args, appConfig, SetupConfigurationDefinition.Fields);
if (!installation.ShouldRunApplication)
{
    Environment.ExitCode = installation.ExitCode;
    return;
}


var builder = Host.CreateApplicationBuilder(args);
builder.AddSerilogLogging();


builder.Services
    .AddOptions<ApplicationOptions>()
    .BindConfiguration(ApplicationOptions.SectionName)
    .Validate(opts => !string.IsNullOrEmpty(opts.Name), "Application:Name must be provided.")
    .Validate(opts => !string.IsNullOrEmpty(opts.ServiceName), "Application:ServiceName must be provided.")
    .ValidateOnStart();

builder.Services
    .AddOptions<PollingOptions>()
    .BindConfiguration(PollingOptions.SectionName)
    .Validate(opts => 
        opts.Fast > TimeSpan.Zero && 
        opts.Medium > TimeSpan.Zero &&
        opts.Slow > TimeSpan.Zero, "PollingOptions:Fast, PollingOptions:Medium, and PollingOptions:Slow must be greater than zero.")
    .Validate(opts => 
        opts.Fast <= opts.Medium &&
        opts.Medium <= opts.Slow, "PollingOptions:Fast must be less than PollingOptions:Medium, and PollingOptions:Medium must be less than PollingOptions:Slow.")
    .ValidateOnStart();

var appOptions = builder.Configuration
    .GetRequiredSection(ApplicationOptions.SectionName)
    .Get<ApplicationOptions>() ?? throw new InvalidOperationException($"Missing configuration section '{ApplicationOptions.SectionName}'.");



builder.Services
    .AddHostedService<SensorPollingService>()
    .AddHostedService<DeviceRegistrationService>()
    .AddHostedService<CommandExecutionService>()
    .AddCommunication(builder.Configuration)
    .AddDeviceMonitoring(builder.Configuration);


builder.AddWindowsServiceHosting(appOptions.ServiceName);

var app = builder.Build();
await app.RunAsync();