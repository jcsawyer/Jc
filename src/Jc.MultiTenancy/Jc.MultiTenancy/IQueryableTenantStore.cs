using System.Linq;

namespace Jc.MultiTenancy
{
    /// <summary>
    /// Describes a <see cref="ITenantStore{TTenant}"/> that implements
    /// a <see cref="IQueryable{TTenant}"/>
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public interface IQueryableTenantStore<TTenant> : ITenantStore<TTenant>
        where TTenant : class, ITenant
    {
        /// <summary>
        /// <see cref="IQueryable"/> tenants
        /// </summary>
        IQueryable<TTenant> Tenants { get; }
    }
}
