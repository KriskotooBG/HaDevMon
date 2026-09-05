using DeviceMonitoring.Abstractions.Sensors;
using DeviceMonitoring.Generic.Sensors.Uptime;
using HaDevMon.Server;
using HaDevMon.Server.Configuration;
using HaDevMon.Server.DependencyInjection;
using HaDevMon.Server.Logging;
using Hosting.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder(args);
builder.AddSerilogLogging();


builder.Services
    .AddOptions<ApplicationOptions>()
    .BindConfiguration(ApplicationOptions.SectionName)
    .Validate(opts => !string.IsNullOrEmpty(opts.Name), "Application:Name must be provided.")
    .Validate(opts => !string.IsNullOrEmpty(opts.ServiceName), "Application:ServiceName must be provided.")
    .ValidateOnStart();

var appOptions = builder.Configuration
    .GetRequiredSection(ApplicationOptions.SectionName)
    .Get<ApplicationOptions>() ?? throw new InvalidOperationException($"Missing configuration section '{ApplicationOptions.SectionName}'.");

builder.Services
    .AddIfEnabled<ISensorContributor, UptimeSensor>(builder.Configuration, "Features:Sensors:Uptime");


builder.Services
    .AddHostedService<HaDevMonTestWorker>();


builder.AddWindowsServiceHosting(appOptions.ServiceName);

var app = builder.Build();
await app.RunAsync();