using System;
using System.Diagnostics.CodeAnalysis;

namespace Jc.MultiTenancy.AspNetCore
{
    /// <summary>
    /// Represents a <typeparamref name="TTenant"/> context
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantContext<TTenant>
        where TTenant : class, ITenant
    {
        /// <summary>
        /// Initializes a <see cref="TenantContext{TTenant}"/> for the 
        /// given <paramref name="tenant"/>
        /// </summary>
        /// <param name="tenant"><typeparamref name="TTenant"/> tenant</param>
        public TenantContext([NotNull] TTenant tenant)
            => Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));

        /// <summary>
        /// Gets the context unique identifier
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the tenant instance
        /// </summary>
        public TTenant Tenant { get; }
    }
}
