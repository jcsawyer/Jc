using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Jc.Core;

namespace Jc.MultiTenancy
{
    /// <summary>
    /// Provides APIs for managing <typeparamref name="TTenant"/>s in 
    /// a persistence store
    /// </summary>
    /// <typeparam name="TTenant">The tenant type</typeparam>
    public class TenantManager<TTenant> : IDisposable
        where TTenant : class, ITenant
    {
        private bool _isDisposed;

        /// <summary>
        /// A shared <see cref="CancellationToken"/> for all <see cref="TenantManager{TTenant}"/>
        /// asynchronous operations
        /// </summary>
        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        /// <summary>
        /// Initializes a <see cref="TenantManager{TTenant}"/> using the specified <paramref name="store"/>
        /// with the given <paramref name="options"/> and <paramref name="logger"/>
        /// </summary>
        /// <param name="store"><see cref="ITenantStore{TTenant}"/> persistent store</param>
        /// <param name="options"><see cref="MultiTenancyOptions"/> options</param>
        /// <param name="logger"><see cref="ILogger{TenantManager{TTenant}}"/> logger</param>
        public TenantManager(
            ITenantStore<TTenant> store,
            IOptions<MultiTenancyOptions> options,
            ILogger<TenantManager<TTenant>> logger)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Options = options?.Value ?? new MultiTenancyOptions();
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the persistent <typeparamref name="TTenant"/> store
        /// </summary>
        protected ITenantStore<TTenant> Store { get; set; }

        /// <summary>
        /// Gets or sets the multi tenancy options
        /// </summary>
        public MultiTenancyOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the logger
        /// </summary>
        public virtual ILogger<TenantManager<TTenant>> Logger { get; set; }

        /// <inheritdoc cref="IQueryableTenantStore{TTenant}.Tenants"/>
        public virtual IQueryable<TTenant> Tenants
        {
            get
            {
                var queryableStore = Store as IQueryableTenantStore<TTenant>;
                if (queryableStore == null)
                    throw new NotSupportedException("Store does not implement IQueryableTenantStore<TTenant>");

                return queryableStore.Tenants;
            }
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.CreateAsync(TTenant, CancellationToken)"/>
        public virtual Task<JcResult> CreateAsync([NotNull] TTenant tenant)
        {
            ThrowIfDisposed();
            return Store.CreateAsync(tenant, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.UpdateAsync(TTenant, CancellationToken)"/>
        public virtual Task<JcResult> UpdateAsync([NotNull] TTenant tenant)
        {
            ThrowIfDisposed();
            return Store.UpdateAsync(tenant, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.DeleteAsync(TTenant, CancellationToken)"/>
        public virtual Task<JcResult> DeleteAsync([NotNull] TTenant tenant)
        {
            ThrowIfDisposed();
            return Store.DeleteAsync(tenant, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.FindByIdAsync(Guid, CancellationToken)"/>
        public virtual Task<TTenant> FindByIdAsync(Guid id)
        {
            ThrowIfDisposed();
            return Store.FindByIdAsync(id, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.FindByNameAsync(string, CancellationToken)"/>
        public virtual Task<TTenant> FindByNameAsync(string name)
        {
            ThrowIfDisposed();
            return Store.FindByNameAsync(name, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.FindByHostAsync(string, CancellationToken)"/>
        public virtual Task<TTenant> FindByHostAsync(string host)
        {
            ThrowIfDisposed();
            return Store.FindByHostAsync(host, CancellationToken);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.SetNameAsync(TTenant, string, CancellationToken)"/>
        public virtual async Task<JcResult> SetNameAsync([NotNull] TTenant tenant, [NotNull] string name)
        {
            ThrowIfDisposed();
            await Store.SetNameAsync(tenant, name, CancellationToken);

            return await UpdateAsync(tenant);
        }

        /// <inheritdoc cref="ITenantStore{TTenant}.SetHostAsync(TTenant, string, CancellationToken)"/>
        public virtual async Task<JcResult> SetHostAsync([NotNull] TTenant tenant, [NotNull] string host)
        {
            ThrowIfDisposed();
            await Store.SetHostAsync(tenant, host, CancellationToken);

            return await UpdateAsync(tenant);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Dispose"/>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing && !_isDisposed)
            {
                Store.Dispose();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if an operation is
        /// started when the current <see cref="TenantStoreBase{TTenant}"/> is disposed
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
