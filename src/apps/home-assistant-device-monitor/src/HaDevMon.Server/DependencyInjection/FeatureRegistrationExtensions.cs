using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HaDevMon.Server.DependencyInjection
{
    public static class FeatureRegistrationExtensions
    {
        public static IServiceCollection AddIfEnabled<TService, TImplementation>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionPath
        ) where TService : class where TImplementation : class, TService
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sectionPath);

            var section = configuration.GetRequiredSection(sectionPath);

            if (section.GetValue<bool>("Enabled"))
                services.AddSingleton<TService, TImplementation>();

            return services;
        }
    }
}
