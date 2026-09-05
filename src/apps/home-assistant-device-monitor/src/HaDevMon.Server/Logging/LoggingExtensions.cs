using Microsoft.Extensions.Hosting;
using Serilog;


namespace HaDevMon.Server.Logging
{
    public static class LoggingExtensions
    {
        public static IHostApplicationBuilder AddSerilogLogging(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            return builder;
        }
    }
}
