using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

using Jc.MultiTenancy;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> multi tenancy extensions
    /// </summary>
    public static class MultiTenancyServiceCollectionExtensions
    {
        /// <summary>
        /// Adds multi tenancy to the specified <paramref name="services"/>
        /// for the given <typeparamref name="TTenant"/> type with the default
        /// <see cref="MultiTenancyOptions"/>
        /// </summary>
        /// <typeparam name="TTenant">Type of tenants</typeparam>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddMultiTenancy<TTenant>(this IServiceCollection services)
            where TTenant : class, ITenant
        {
            return services.AddMultiTenancy<TTenant>(options => { });
        }

        /// <summary>
        /// Adds multi tenancy to the specified <paramref name="services"/>
        /// for the given <typeparamref name="TTenant"/> type using the
        /// given <paramref name="optionsAction"/> options
        /// </summary>
        /// <typeparam name="TTenant">Type of tenants</typeparam>
        /// <param name="setupAction"><see cref="Action{MultiTenancyOptions}"/> options action</param>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddMultiTenancy<TTenant>(this IServiceCollection services, Action<MultiTenancyOptions> optionsAction)
            where TTenant : class, ITenant
        {
            services.AddOptions();

            services.TryAddScoped<TenantManager<TTenant>>();

            if (optionsAction != null)
                services.Configure(optionsAction);

            return new MultiTenancyBuilder(typeof(TTenant), services);
        }
    }
}
