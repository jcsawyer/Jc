using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

using Jc.MultiTenancy;
using Jc.MultiTenancy.AspNetCore;
using Jc.MultiTenancy.Caching;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="MultiTenancyBuilder"/> aspnetcore extensions
    /// </summary>
    public static class MultiTenancyBuilderExtensions
    {
        /// <summary>
        /// Adds the tenant resolver of the specified <typeparamref name="TResolver"/> type 
        /// to the <see cref="MultiTenancyBuilder"/>
        /// </summary>
        /// <typeparam name="TResolver">Type of <see cref="ITenantResolver{TTenant}"/></typeparam>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddHostResolver<TResolver>(this MultiTenancyBuilder builder)
            where TResolver : class
        {
            AddResolverServices(builder.Services, builder.TenantType, typeof(TResolver));
            return builder;
        }

        /// <summary>
        /// Adds the aspnetcore host header resolver to the <see cref="MultiTenancyBuilder"/>
        /// </summary>
        /// <returns><see cref="MultiTenancyBuilder"/> builder</returns>
        public static MultiTenancyBuilder AddHostResolver(this MultiTenancyBuilder builder)
        {
            AddResolverServices(builder.Services, builder.TenantType, typeof(HostTenantResolver<>).MakeGenericType(builder.TenantType));
            return builder;
        }

        /// <summary>
        /// Adds the <paramref name="resolverType"/> services for <paramref name="tenantType"/> 
        /// to the given <paramref name="services"/> collection
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> services</param>
        /// <param name="tenantType">Tenant <see cref="Type"/></param>
        /// <param name="resolverType">Tenant resolver <see cref="Type"/></param>
        private static void AddResolverServices(
            IServiceCollection services,
            Type tenantType,
            Type resolverType)
        {
            services.AddScoped(typeof(ITenantResolver<>).MakeGenericType(tenantType), resolverType);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (typeof(MemoryCacheTenantResolver<>).MakeGenericType(tenantType).IsAssignableFrom(resolverType))
                services.AddMemoryCache();
        }
    }
}
