namespace Jc.Core
{
    /// <summary>
    /// Encapsulates an error from a Jc operation
    /// </summary>
    public class JcError
    {
        /// <summary>
        /// Gets or sets the code for this error
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the description for this error
        /// </summary>
        public string Description { get; set; }
    }
}
