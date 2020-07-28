using System;

namespace Jc.MultiTenancy
{
    /// <summary>
    /// Describes a tenant
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// Gets or sets the <see cref="ITenant"/> unique identifier
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITenant"/> name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITenant"/> host name
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="ITenant"/> is active
        /// </summary>
        bool IsActive { get; set; }
    }
}
