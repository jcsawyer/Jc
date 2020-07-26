using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy
{
    /// <summary>
    /// Desctibes a method of resolving the <typeparamref name="TTenant"/> 
    /// in the current context
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public interface ITenantResolver<TTenant>
        where TTenant : class, ITenant
    {
        /// <summary>
        /// Resolves the <typeparamref name="TTenant"/> for the current context
        /// with the given <paramref name="cancellationToken"/>
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns>The resolved <typeparamref name="TTenant"/> or null if not found</returns>
        Task<TTenant> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
