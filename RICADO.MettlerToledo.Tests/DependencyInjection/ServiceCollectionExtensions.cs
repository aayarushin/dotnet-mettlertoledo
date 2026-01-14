using System;
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.Channels;
using RICADO.MettlerToledo.Tests.Mocks;

namespace RICADO.MettlerToledo.Tests.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Mettler Toledo test services with dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Add Mettler Toledo services to the service collection with a custom mock channel factory
        /// Allows tests to provide custom mock channel behaviors
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="mockChannelFactory">Custom mock channel factory instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMettlerToledoMocks(this IServiceCollection services, MockChannelFactory mockChannelFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mockChannelFactory == null)
            {
                throw new ArgumentNullException(nameof(mockChannelFactory));
            }

            services.AddSingleton<IChannelFactory>(mockChannelFactory);

            services.AddSingleton<IMettlerToledoDeviceFactory, MettlerToledoDeviceFactory>();

            return services;
        }
    }
}
