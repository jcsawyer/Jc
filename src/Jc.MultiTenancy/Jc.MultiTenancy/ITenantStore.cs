using System;
using System.Threading;
using System.Threading.Tasks;

using Jc.Core;

namespace Jc.MultiTenancy
{
    /// <summary>
    /// Describes a store for of the specified <typeparamref name="TTenant"/> type
    /// </summary>
    /// <typeparam name="TTenant">The implementation type of <see cref="ITenant"/></typeparam>
    public interface ITenantStore<TTenant> : IDisposable
        where TTenant : class, ITenant
    {
        /// <summary>
        /// Creates a new <typeparamref name="TTenant"/> in the store with the optional
        /// <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> to add</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns><see cref="JcResult"/> create operation result</returns>
        Task<JcResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the given <typeparamref name="TTenant"/> in the store with the
        /// optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> to update</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns><see cref="JcResult"/> update operation result</returns>
        Task<JcResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the given <typeparamref name="TTenant"/> from the store
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> to delete</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns><see cref="JcResult"/> delete operation result</returns>
        Task<JcResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a <typeparamref name="TTenant"/> in the store with the specified
        /// <paramref name="id"/> with the optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="id">Tenant id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// The <typeparamref name="TTenant"/> with the specified <paramref name="id"/>
        /// or null if it does not exist
        /// </returns>
        Task<TTenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a <typeparamref name="TTenant"/> in the store with the specified
        /// <paramref name="name"/> with the optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="name"><typeparamref name="TTenant"/> name</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>
        /// The <typeparamref name="TTenant"/> with the specified <paramref name="name"/>
        /// or null if it does not exist
        /// </returns>
        Task<TTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a <typeparamref name="TTenant"/> in the store with the specified
        /// <paramref name="host"/> with the optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="host"><typeparamref name="TTenant"/> host</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>
        /// The <typeparamref name="TTenant"/> with the specified <paramref name="host"/>
        /// or null if it does not exist
        /// </returns>
        Task<TTenant> FindByHostAsync(string host, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the name of the given <paramref name="tenant"/> to the specified
        /// <paramref name="name"/> with the optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> to set name</param>
        /// <param name="name">Name to set for <typeparamref name="TTenant"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>awaitable <see cref="Task"/></returns>
        Task SetNameAsync(TTenant tenant, string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the name of the given <paramref name="tenant"/> to the specified
        /// <paramref name="host"/> with the optional <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> to set host</param>
        /// <param name="host">Host to set for <typeparamref name="TTenant"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>awaitable <see cref="Task"/></returns>
        Task SetHostAsync(TTenant tenant, string host, CancellationToken cancellationToken = default);
    }
}
