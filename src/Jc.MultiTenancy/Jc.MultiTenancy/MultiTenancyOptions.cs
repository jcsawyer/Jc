namespace Jc.MultiTenancy
{
    /// <summary>
    /// Represents options for Jc application multi tenancy
    /// </summary>
    public class MultiTenancyOptions
    {
        public MultiTenancyUnresolvedOptions Unresolved { get; set; } = new MultiTenancyUnresolvedOptions();
    }

    /// <summary>
    /// Represents unresolved tenant options for Jc multi tenancy
    /// </summary>
    public class MultiTenancyUnresolvedOptions
    {
        /// <summary>
        /// Gets or sets whether the unresolved redirect should use a HTTP 301
        /// (moved permanently) when <c>true</c> or HTTP 302 (found) when <c>false</c>
        /// </summary>
        public bool IsPermanentRedirect { get; set; }

        /// <summary>
        /// Gets or sets the redirect url for when a tenant cannot be resolved
        /// </summary>
        public string RedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the redirect url for when a resolved tenant is inactive.
        /// If not set, the <see cref="RedirectUrl"/> will be used by default
        /// </summary>
        public string InactiveRedirectUrl { get; set; } = string.Empty;
    }
}
