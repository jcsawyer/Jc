using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

using Jc.MultiTenancy;
using Jc.MultiTenancy.EntityFramework;
using Jc.MultiTenancy.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="MultiTenancyBuilder"/> entity framework extensions
    /// </summary>
    public static class MultiTenancyBuilderExtensions
    {
        /// <summary>
        /// Adds the entity framework tenant store of the specified 
        /// <typeparamref name="TContext"/> type
        /// </summary>
        /// <typeparam name="TContext">Type of <see cref="TenantDbContext"/></typeparam>
        /// <param name="optionsAction"><see cref="Action{DbContextOptionsBuilder}"/> options action</param>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddEntityFrameworkStore<TContext>(
            this MultiTenancyBuilder builder,
            Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            builder.Services.AddDbContext<TContext>(optionsAction);
            AddStores(builder.Services, builder.TenantType, typeof(TContext));
            return builder;
        }

        /// <summary>
        /// Adds the entity framework tenant store for the default <see cref="Tenant"/>
        /// type
        /// </summary>
        /// <param name="optionsAction"><see cref="Action{DbContextOptions}"/> options action</param>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddEntityFrameworkStore(
            this MultiTenancyBuilder builder,
            Action<DbContextOptionsBuilder> optionsAction = null)
        {
            builder.Services.AddDbContext<TenantDbContext>(optionsAction);
            AddStores(builder.Services, builder.TenantType, typeof(TenantDbContext));
            return builder;
        }

        /// <summary>
        /// Adds the store services for <paramref name="tenantType"/> with the
        /// specified <paramref name="contextType"/> to the <paramref name="services"/> 
        /// collection
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> services</param>
        /// <param name="tenantType"><see cref="Type"/> of tenant</param>
        /// <param name="contextType"><see cref="Type"/> of <see cref="TenantDbContext"/></param>
        private static void AddStores(
            IServiceCollection services,
            Type tenantType,
            Type contextType)
        {
            Type tenantStoreType = typeof(TenantStore<,>).MakeGenericType(tenantType, contextType);

            services.TryAddScoped(typeof(ITenantStore<>).MakeGenericType(tenantType), tenantStoreType);
        }
    }
}
