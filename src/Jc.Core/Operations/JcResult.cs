using System.Collections.Generic;
using System.Linq;

namespace Jc.Core
{
    /// <summary>
    /// Represents the result of a JcCMS operation
    /// </summary>
    public class JcResult
    {
        protected static readonly JcResult success = new JcResult { Succeeded = true };
        protected List<JcError> errors = new List<JcError>();

        /// <summary>
        /// Flag indicating whether the operation succeeded or not
        /// </summary>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="JcError"/>s containing any
        /// errors that occured during the operation
        /// </summary>
        public IEnumerable<JcError> Errors => errors;

        /// <summary>
        /// Returns a <see cref="JcResult"/> indicating a successful operation
        /// </summary>
        public static JcResult Success => success;

        /// <summary>
        /// Returns a <see cref="JcResult"/> indicating a failed operation,
        /// with a list of <paramref name="errors"/> if applicable
        /// </summary>
        /// <param name="errors">An optional array of <see cref="JcError"/>s which caused 
        /// the operation to fail</param>
        /// <returns>A <see cref="JcResult"/> indicating a failed operation</returns>
        public static JcResult Failed(params JcError[] errors)
        {
            var result = new JcResult { Succeeded = false };
            if (errors != null)
                result.errors.AddRange(errors);

            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="JcResult"/> to its equivalent string
        /// representation
        /// </summary>
        /// <returns>A string representation of the current <see cref="JcResult"/></returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it
        /// returns "Failed : " followed by a comma delimited list of error codes from its
        /// <see cref="Errors"/> collection if any
        /// </remarks>
        public override string ToString()
            => Succeeded
                ? "Succeeded"
                : $"Failed: {string.Join(",", Errors.Select(x => x.Code).ToList())}";
    }
}
