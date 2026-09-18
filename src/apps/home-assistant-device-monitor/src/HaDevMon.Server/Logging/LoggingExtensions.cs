using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;


namespace HaDevMon.Server.Logging
{
    public static class LoggingExtensions
    {
        public static IHostApplicationBuilder AddSerilogLogging(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, configuration) =>
            {
                var readerOptions = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly);

                configuration
                    .ReadFrom.Configuration(builder.Configuration, readerOptions)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            return builder;
        }
    }
}
