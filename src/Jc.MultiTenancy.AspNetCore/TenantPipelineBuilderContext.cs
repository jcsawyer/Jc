namespace Jc.MultiTenancy.AspNetCore
{
    /// <summary>
    /// Context for tenant pipline building
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantPipelineBuilderContext<TTenant>
        where TTenant : class, ITenant
    {
        /// <summary>
        /// Gets or sets the <see cref="TenantContext"/>
        /// </summary>
        public TenantContext<TTenant> TenantContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Tenant"/>
        /// </summary>
        public TTenant Tenant { get; set; }
    }
}
