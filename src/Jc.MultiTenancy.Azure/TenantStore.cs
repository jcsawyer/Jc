using Azure.Storage.Blobs;
using Jc.Core;
using Jc.MultiTenancy.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Azure
{
    /// <summary>
    /// A persistent azure blob store for <see cref="Tenant"/>s
    /// </summary>
    public class TenantStore : TenantStore<Tenant>
    {
        /// <summary>
        /// Initializes a <see cref="TenantStore"/> with the given
        /// blob storage <paramref name="options"/>
        /// </summary>
        /// <param name="options"><see cref="BlobTenantStoreOptions"/> blob options</param>
        public TenantStore([NotNull]BlobTenantStoreOptions options) : base(options) { }
    }

    /// <summary>
    /// A persistent azure blob store for <typeparamref name="TTenant"/>s
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantStore<TTenant> : ITenantStore<TTenant>
        where TTenant : Tenant
    {
        private readonly BlobTenantStoreOptions _options;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a <see cref="TenantStore"/> for <typeparamref name="TTenant"/>s
        /// with the given blob storage <paramref name="options"/>
        /// </summary>
        /// <param name="options"><see cref="BlobTenantStoreOptions"/> blob options</param>
        public TenantStore([NotNull]BlobTenantStoreOptions options)
            => _options = options;

        /// <inheritdoc/>
        public async Task<JcResult> CreateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            
            tenants.Add(tenant);
            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);
            
            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<JcResult> UpdateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            var tenantToUpdate = tenants.SingleOrDefault(x => x.Id == tenant.Id);

            tenantToUpdate = tenant;
            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);
            
            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<JcResult> DeleteAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            var tenantToDelete = tenants.SingleOrDefault(x => x.Id == tenant.Id);

            tenants.Remove(tenantToDelete);
            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);

            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            return tenants.FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            return tenants.FirstOrDefault(x => x.Name == name);
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByHostAsync(string host, CancellationToken cancellationToken = default)
        {
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            return tenants.FirstOrDefault(x => x.Host == host);
        }

        /// <inheritdoc/>
        public virtual Task SetNameAsync(
            [NotNull] TTenant tenant,
            string name,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            tenant.Name = name;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task SetHostAsync(
            [NotNull] TTenant tenant,
            string host,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            tenant.Host = host;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Loads all <typeparamref name="TTenant"/>s from azure blob storage
        /// </summary>
        /// <returns><see cref="IEnumerable{TTenant}"/> tenants</returns>
        protected virtual async Task<IEnumerable<TTenant>> LoadTenantsFromBlobStorageAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var serviceClient = new BlobServiceClient(_options.ConnectionString);
            var container = serviceClient.GetBlobContainerClient(_options.ContainerName);
            var blob = container.GetBlobClient(_options.BlobName);
            
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToAsync(stream);
                var tenants = await JsonSerializer.DeserializeAsync<List<TTenant>>(stream);

                return tenants;
            }
        }

        /// <summary>
        /// Saves all <paramref name="tenants"/> to azure blob storage
        /// </summary>
        /// <param name="tenants"><see cref="IEnumerable{TTenant}"/> tenats</param>
        /// <returns>An await <see cref="Task"/></returns>
        protected virtual async Task SaveTenantsToBlobStorageAsync(IEnumerable<TTenant> tenants, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var serviceClient = new BlobServiceClient(_options.ConnectionString);
            var container = serviceClient.GetBlobContainerClient(_options.ContainerName);
            var blob = container.GetBlobClient(_options.BlobName);
            
            using (var stream = new MemoryStream())
            {
                await blob.UploadAsync(stream);
                await JsonSerializer.SerializeAsync(stream, tenants);
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if an operation is
        /// started when the current <see cref="TenantStore{TTenant}"/> is disposed
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <inheritdoc/>
        public void Dispose()
            => _isDisposed = true;
    }
}
