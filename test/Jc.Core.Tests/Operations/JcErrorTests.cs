using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Jc.Core.Tests.Operations
{
    [TestClass]
    public class JcErrorTests
    {
        [TestMethod]
        public void Constructor_Sets_Code()
        {
            var code = "T35T";

            var error = new JcError(code, "");

            Assert.AreEqual(code, error.Code);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_Null_Code()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new JcError(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("code", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_Empty_String_Code()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new JcError(""));

            Assert.IsNotNull(exception);
            Assert.AreEqual("code", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Sets_Description()
        {
            var description = "Test description";

            var error = new JcError("T35T", description);

            Assert.AreEqual(description, error.Description);
        }
    }
}
