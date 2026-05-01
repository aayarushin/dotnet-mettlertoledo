using System;
using Microsoft.Extensions.DependencyInjection;
using Omda.MettlerToledo.Channels;

namespace Omda.MettlerToledo.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Mettler Toledo services with dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Mettler Toledo services to the service collection for production use
        /// Registers IChannelFactory and IMettlerToledoDeviceFactory with their production implementations
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMettlerToledo(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IChannelFactory, ChannelFactory>();
            services.AddSingleton<IMettlerToledoDeviceFactory, MettlerToledoDeviceFactory>();

            return services;
        }
    }
}
