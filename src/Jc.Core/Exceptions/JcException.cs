using System;

namespace Jc.Core
{
    /// <summary>
    /// Represents an application exception from a Jc application
    /// </summary>
    public class JcException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="JcException"/> application execption
        /// </summary>
        public JcException() { }

        /// <summary>
        /// Initializes a new <see cref="JcException"/> application exception with
        /// the specified <paramref name="message"/>
        /// </summary>
        /// <param name="message">Exception message</param>
        public JcException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new <see cref="JcException"/> application exception with
        /// the specified <paramref name="message"/> and <paramref name="innerException"/>
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner <see cref="Exception"/></param>
        public JcException(string message, Exception innerException) : base(message, innerException) { }
    }
}
