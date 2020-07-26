namespace Jc.MultiTenancy.Caching
{
    /// <summary>
    /// Represents the options for the <see cref="MemoryCacheTenantResolver"/>
    /// </summary>
    public class MemoryCacheTenantResolverOptions
    {
        /// <summary>
        /// Initialises a new <see cref="MemoryCacheTenantResolverOptions"/>
        /// </summary>
        public MemoryCacheTenantResolverOptions() { }

        /// <summary>
        /// Gets or sets whether all cache entries should be eviced on expiry
        /// </summary>
        /// <value><c>true</c> to evice all on expiry. Defaults to <c>true</c></value>
        public bool EvictAllEntriesOnExpiry { get; set; } = true;
    }
}
