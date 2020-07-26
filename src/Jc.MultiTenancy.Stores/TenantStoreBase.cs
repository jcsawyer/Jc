using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Jc.Core;

namespace Jc.MultiTenancy.Stores
{
    /// <summary>
    /// A base store for <typeparamref name="TTenant"/>s 
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public abstract class TenantStoreBase<TTenant> : ITenantStore<TTenant>, IQueryableTenantStore<TTenant>
        where TTenant : Tenant
    {
        private bool _isDisposed;

        /// <inheritdoc/>
        public abstract Task<JcResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<JcResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<JcResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<TTenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<TTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<TTenant> FindByHostAsync(string host, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract IQueryable<TTenant> Tenants { get; }

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
        /// Throws an <see cref="ObjectDisposedException"/> if an operation is
        /// started when the current <see cref="TenantStoreBase{TTenant}"/> is disposed
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
