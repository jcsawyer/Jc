namespace Jc.MultiTenancy.Azure
{
    /// <summary>
    /// Represents Azure blob store options
    /// </summary>
    public class BlobTenantStoreOptions
    {
        /// <summary>
        /// Gets or sets the blob connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the blob container name
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the blob name
        /// </summary>
        public string BlobName { get; set; }
    }
}
