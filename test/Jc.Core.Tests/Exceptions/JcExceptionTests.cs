using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Jc.Core.Tests.Exceptions
{
    [TestClass]
    public class JcExceptionTests
    {
        [TestMethod]
        public void JcException_Is_Type_Of_Exception()
        {
            var exception = new JcException();

            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public void Jc_Exception_Can_Be_Thrown()
        {
            Assert.ThrowsException<JcException>(() => throw new JcException());
        }

        [TestMethod]
        public void Constructor_Sets_Message()
        {
            var message = "Test Message";
            
            var exception = new JcException(message);

            Assert.AreEqual(message, exception.Message);
        }

        [TestMethod]
        public void Constructor_Sets_Message_And_Inner_Exception()
        {
            var message = "Test Message";
            var innerException = new InvalidOperationException();

            var exception = new JcException(message, innerException);

            Assert.AreEqual(message, exception.Message);
            Assert.AreSame(innerException, exception.InnerException);
        }
    }
}
