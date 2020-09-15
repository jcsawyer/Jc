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
        /// Initializes a <see cref="TenantStore"/> with the given <paramref name="client"/>
        /// and blob storage <paramref name="options"/>
        /// </summary>
        /// <param name="client"><see cref="BlobServiceClient"/> blob client</param>
        /// <param name="options"><see cref="BlobTenantStoreOptions"/> blob options</param>
        public TenantStore(
            [NotNull]BlobServiceClient client,
            [NotNull]BlobTenantStoreOptions options) : base(client, options) { }
    }

    /// <summary>
    /// A persistent azure blob store for <typeparamref name="TTenant"/>s
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantStore<TTenant> : ITenantStore<TTenant>
        where TTenant : Tenant
    {
        private const string _code = "102";
        private readonly BlobServiceClient _client;
        private readonly BlobTenantStoreOptions _options;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a <see cref="TenantStore"/> for <typeparamref name="TTenant"/>s
        /// with the given <paramref name="client"/> and blob storage <paramref name="options"/>
        /// </summary>
        /// <param name="client"><see cref="BlobServiceClient"/> blob client</param>
        /// <param name="options"><see cref="BlobTenantStoreOptions"/> blob options</param>
        public TenantStore(
            [NotNull]BlobServiceClient client,
            [NotNull]BlobTenantStoreOptions options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public async Task<JcResult> CreateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();

            if (tenants.Any(x => x.Name.Equals(tenant.Name) && x.Host.Equals(tenant.Host)))
                return JcResult.Failed(new JcError($"{_code}101", $"Tenant with name {tenant.Name} and host {tenant.Host} already exists"));
            else if (tenants.Any(x => x.Name.Equals(tenant.Name)))
                return JcResult.Failed(new JcError($"{_code}102", $"Tenant with name {tenant.Name} already exists"));
            else if (tenants.Any(x => x.Host.Equals(tenant.Host)))
                return JcResult.Failed(new JcError($"{_code}103", $"Tenant with host {tenant.Host} already exists"));

            tenants.Add(tenant);
            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);
            
            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<JcResult> UpdateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            
            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            var matchingTenants = tenants.Where(x => x.Id == tenant.Id || x.Name == tenant.Name || x.Host == tenant.Host);

            if (matchingTenants.Count() == 0)
                return JcResult.Failed(new JcError($"{_code}201", "Tenant not found"));
            else if (matchingTenants.Count() > 1)
                return JcResult.Failed(new JcError($"{_code}202", "Multiple tenants found"));

            var tenantToUpdate = matchingTenants.First();
            tenantToUpdate.Id = tenant.Id;
            tenantToUpdate.Name = tenant.Name;
            tenantToUpdate.Host = tenant.Host;
            tenantToUpdate.IsActive = tenant.IsActive;

            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);
            
            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<JcResult> DeleteAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            var matchingTenants = tenants.Where(x => x.Id == tenant.Id || x.Name == tenant.Name || x.Host == tenant.Host);

            if (matchingTenants.Count() == 0)
                return JcResult.Failed(new JcError($"{_code}301", "Tenant not found"));
            else if (matchingTenants.Count() > 1)
                return JcResult.Failed(new JcError($"{_code}302", "Multiple tenants found"));

            tenants.Remove(matchingTenants.First());
            await SaveTenantsToBlobStorageAsync(tenants, cancellationToken);

            return JcResult.Success;
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            return tenants.FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenants = (await LoadTenantsFromBlobStorageAsync(cancellationToken)).ToList();
            return tenants.FirstOrDefault(x => x.Name == name);
        }

        /// <inheritdoc/>
        public async Task<TTenant> FindByHostAsync(string host, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

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

            var container = _client.GetBlobContainerClient(_options.ContainerName);
            var blob = container.GetBlobClient(_options.BlobName);

            if (!await blob.ExistsAsync(cancellationToken))
                return new List<TTenant>();

            var download = await blob.DownloadAsync(cancellationToken);
            using (var stream = new MemoryStream())
            {
                var tenants = await JsonSerializer.DeserializeAsync<List<TTenant>>(download.Value.Content, cancellationToken: cancellationToken);
                
                return tenants;
            }
        }

        /// <summary>
        /// Saves all <paramref name="tenants"/> to azure blob storage
        /// </summary>
        /// <param name="tenants"><see cref="IEnumerable{TTenant}"/> tenats</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        protected virtual async Task SaveTenantsToBlobStorageAsync(IEnumerable<TTenant> tenants, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var container = _client.GetBlobContainerClient(_options.ContainerName);
            var blob = container.GetBlobClient(_options.BlobName);

            var data = JsonSerializer.SerializeToUtf8Bytes(tenants);

            using (var stream = new MemoryStream(data))
                await blob.UploadAsync(stream, overwrite: true, cancellationToken);
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
