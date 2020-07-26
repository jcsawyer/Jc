using System;

namespace Jc.MultiTenancy.Stores
{
    /// <summary>
    /// Represents a tenant
    /// </summary>
    public class Tenant : ITenant
    {
        /// <summary>
        /// Gets or sets the <see cref="Tenant"/> unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Tenant"/> name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Tenant"/> host name
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="Tenant"/> is active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
