using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

using Jc.MultiTenancy;
using Jc.MultiTenancy.Azure;
using Azure.Storage.Blobs;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="MultiTenancyBuilder"/> azure extensions
    /// </summary>
    public static class MultiTenancyBuilderExtensions
    {
        /// <summary>
        /// Adds the azure blob store for tennts
        /// </summary>
        /// <param name="optionsAction"><see cref="Action{BlobTenantStoreOptions}"/> options action</param>
        /// <returns></returns>
        public static MultiTenancyBuilder AddAzureBlobStore(
            this MultiTenancyBuilder builder,
            Action<BlobTenantStoreOptions> optionsAction = null)
        {
            AddStores(builder.Services, builder.TenantType, optionsAction);
            return builder;
        }

        /// <summary>
        /// Adds the store services for <paramref name="tenantType"/> with
        /// the specified <paramref name="optionsAction"/> to the <paramref name="services"/>
        /// collection
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> services</param>
        /// <param name="tenantType"><see cref="Type"/> of tenant</param>
        /// <param name="optionsAction"><see cref="Action{BlobTenantStoreOptions}"/> options action</param>
        private static void AddStores(
            [NotNull] IServiceCollection services,
            [NotNull] Type tenantType,
            Action<BlobTenantStoreOptions> optionsAction)
        {
            Type tenantStoreType = typeof(TenantStore<>).MakeGenericType(tenantType);

            var options = new BlobTenantStoreOptions();
            optionsAction?.Invoke(options);

            services.TryAddScoped(services => options);
            services.TryAddScoped(typeof(BlobServiceClient), (services) => {
                var options = services.GetRequiredService<BlobTenantStoreOptions>();
                return new BlobServiceClient(options.ConnectionString);
            });
            services.TryAddScoped(typeof(ITenantStore<>).MakeGenericType(tenantType), tenantStoreType);
        }
    }
}
