using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Jc.Core;
using Jc.MultiTenancy.Stores;

namespace Jc.MultiTenancy.EntityFramework
{
    /// <summary>
    /// A persistent entity framework store for <see cref="Tenant"/>s
    /// </summary>
    public class TenantStore : TenantStore<Tenant>
    {
        /// <summary>
        /// Initializes a <see cref="TenantStore"/> with specified
        /// <paramref name="context"/>
        /// </summary>
        /// <param name="context"><see cref="DbContext"/> tenant database context</param>
        public TenantStore([NotNull] DbContext context) : base(context) { }
    }

    /// <summary>
    /// A persistent entity framework store for <typeparamref name="TTenant"/>s
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantStore<TTenant> : TenantStore<TTenant, DbContext>
        where TTenant : Tenant
    {
        /// <summary>
        /// Initializes a <see cref="TenantStore"/> with specified
        /// <paramref name="context"/>
        /// </summary>
        /// <param name="context"><see cref="DbContext"/> tenant database context</param>
        public TenantStore([NotNull]DbContext context) : base(context) { }
    }

    /// <summary>
    /// A persistent entity framework store for <typeparamref name="TTenant"/>s
    /// using the specified <typeparamref name="TContext"/> type
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    /// <typeparam name="TContext">Type of <see cref="DbContext"/></typeparam>
    public class TenantStore<TTenant, TContext> : TenantStoreBase<TTenant>
        where TTenant : Tenant
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a <see cref="TenantStore{TTenant, TContext}"/> with the
        /// specified <paramref name="context"/>
        /// </summary>
        /// <param name="context"><typeparamref name="TContext"/> tenant database context</param>
        public TenantStore([NotNull] TContext context)
            => Context = context ?? throw new ArgumentNullException(nameof(context));

        /// <summary>
        /// Gets or sets the tenant database <typeparamref name="TContext"/>
        /// </summary>
        public TContext Context { get; set; }

        /// <summary>
        /// Gets the <typeparamref name="TTenant"/> context set
        /// </summary>
        private DbSet<TTenant> TenantsSet => Context.Set<TTenant>();

        /// <summary>
        /// Gets or sets whether to automatically save changes to the database
        /// </summary>
        public bool AutoSaveChanges { get; set; } = true;

        /// <summary>
        /// Saves changes to the database.
        /// If <see cref="AutoSaveChanges"/> is set to <c>true</c>, the changes
        /// are persisted to the database automatically. If <c>false</c>, the
        /// changes to the context must be saved manually
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        protected Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;

        /// <inheritdoc/>
        public override IQueryable<TTenant> Tenants => TenantsSet;

        /// <inheritdoc/>
        public override async Task<JcResult> CreateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Add(tenant);
            await SaveChangesAsync(cancellationToken);

            return JcResult.Success;
        }

        /// <inheritdoc/>
        public override async Task<JcResult> UpdateAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Attach(tenant);
            Context.Update(tenant);
            await SaveChangesAsync(cancellationToken);

            return JcResult.Success;
        }

        /// <inheritdoc/>
        public override async Task<JcResult> DeleteAsync([NotNull] TTenant tenant, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Remove(tenant);
            await SaveChangesAsync(cancellationToken);

            return JcResult.Success;
        }

        /// <inheritdoc/>
        public override Task<TTenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return TenantsSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override Task<TTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return TenantsSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<TTenant> FindByHostAsync(string host, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return TenantsSet.FirstOrDefaultAsync(x => x.Host == host, cancellationToken);
        }
    }
}
