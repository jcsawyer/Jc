using System.Diagnostics.CodeAnalysis;

using Jc.MultiTenancy;
using Jc.MultiTenancy.AspNetCore;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// <see cref="HttpContext"/> multi tenancy extensions
    /// </summary>
    public static class MultiTenancyHttpContextExtensions
    {
        /// <summary>
        /// Http context items tenant key
        /// </summary>
        private const string TenantKey = "Jc.Tenant";

        /// <summary>
        /// Sets the http <paramref name="context"/> tenant item to
        /// the specified <paramref name="tenantContext"/>
        /// </summary>
        /// <typeparam name="TTenant">Type of tenant</typeparam>
        /// <param name="tenantContext"><see cref="TenantContext{TTenant}"/> context</param>
        public static void SetTenant<TTenant>(
            this HttpContext context,
            [NotNull]TenantContext<TTenant> tenantContext)
            where TTenant : class, ITenant
        {
            context.Items[TenantKey] = tenantContext;
        }

        /// <summary>
        /// Retrieves the <see cref="TenantContext{TTenant}"/> from
        /// the http <paramref name="context"/>
        /// </summary>
        /// <typeparam name="TTenant">Type of tenant</typeparam>
        /// <returns><see cref="TenantContext{TTenant}"/> context</returns>
        public static TenantContext<TTenant> GetTenantContext<TTenant>(this HttpContext context)
            where TTenant : class, ITenant
        {
            object tenantContext;
            if (context.Items.TryGetValue(TenantKey, out tenantContext))
                return tenantContext as TenantContext<TTenant>;

            return null;
        }

        /// <summary>
        /// Retrieves the <typeparamref name="TTenant"/> from the
        /// http <paramref name="context"/>
        /// </summary>
        /// <typeparam name="TTenant">Type of tenant</typeparam>
        /// <returns><typeparamref name="TTenant"/> tenant</returns>
        public static TTenant GetTenant<TTenant>(this HttpContext context)
            where TTenant : class, ITenant
        {
            var tenantContext = GetTenantContext<TTenant>(context);
            if (tenantContext != null)
                return tenantContext.Tenant;

            return default(TTenant);
        }
    }
}
