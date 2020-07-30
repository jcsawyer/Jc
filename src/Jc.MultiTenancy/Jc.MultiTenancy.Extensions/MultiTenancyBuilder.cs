using System;

using Jc.MultiTenancy;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder for building and configuring multi tenancy
    /// </summary>
    public class MultiTenancyBuilder
    {
        /// <summary>
        /// Initializes a <see cref="MultiTenancyBuilder"/> builder for the specified
        /// <paramref name="tenantType"/> with the given <paramref name="services"/> container
        /// to register multi tenancy services to
        /// </summary>
        /// <param name="tenantType">Tenant <see cref="Type"/></param>
        /// <param name="services"><see cref="IServiceCollection"/> services</param>
        public MultiTenancyBuilder(
            Type tenantType,
            IServiceCollection services)
        {
            TenantType = tenantType ?? throw new ArgumentNullException(nameof(tenantType));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// The <see cref="Type"/> of tenant for multi tenancy
        /// </summary>
        public Type TenantType { get; private set; }

        /// <summary>
        /// The <see cref="IServiceCollection"/> container to register
        /// multi tenancy services to
        /// </summary>
        public IServiceCollection Services { get; private set; }

        /// <summary>
        /// Adds a scoped services of <paramref name="serviceType"/>
        /// using <paramref name="implementationType"/>
        /// </summary>
        /// <param name="serviceType">Service <see cref="Type"/></param>
        /// <param name="implementationType">Service implementation <see cref="Type"/></param>
        /// <returns></returns>
        private MultiTenancyBuilder AddScoped(
            Type serviceType,
            Type implementationType)
        {
            Services.AddScoped(serviceType, implementationType);
            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TStore"/> for tenants
        /// </summary>
        /// <typeparam name="TStore">Tenant store type</typeparam>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public virtual MultiTenancyBuilder AddStore<TStore>()
            where TStore : class
        {
            AddScoped(typeof(ITenantStore<>), typeof(TStore));
            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TResolver"/> for tenant
        /// resolving
        /// </summary>
        /// <typeparam name="TResolver">Tenant resolver type</typeparam>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public virtual MultiTenancyBuilder AddResolver<TResolver>()
            where TResolver : class
        {
            AddScoped(typeof(ITenantResolver<>), typeof(TResolver));
            return this;
        }
    }
}
