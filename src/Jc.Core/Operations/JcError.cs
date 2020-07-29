using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Jc.Core
{
    /// <summary>
    /// Encapsulates an error from a Jc operation
    /// </summary>
    public class JcError
    {
        /// <summary>
        /// Initializes a new <see cref="JcError"/> with the specified
        /// <paramref name="code"/> and optional <paramref name="description"/>
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="description">Optional error description</param>
        public JcError([NotNull]string code, string description = "")
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException(nameof(code));

            Code = code;
            Description = description;
        }

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
