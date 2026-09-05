using Microsoft.Extensions.Hosting;

namespace Hosting.Windows
{
    public static class ModuleConfigurationExtensions
    {
        public static IHostApplicationBuilder AddWindowsServiceHosting(this IHostApplicationBuilder builder, string serviceName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = serviceName;
            });

            return builder;
        }
    }
}
