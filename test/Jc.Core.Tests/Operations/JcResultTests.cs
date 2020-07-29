using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Jc.Core.Tests.Operations
{
    [TestClass]
    public class JcResultTests
    {
        [TestMethod]
        public void Success_Sets_Succeeded_True()
        {
            var result = JcResult.Success;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public void Failed_Sets_Succeeded_False()
        {
            var result = JcResult.Failed();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod]
        public void Failed_Sets_No_Errors_When_Not_Specified()
        {
            var result = JcResult.Failed();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [TestMethod]
        public void Failed_Sets_Errors_When_Specified()
        {
            var errors = new JcError[]
            {
                new JcError("001", "Error 1"),
                new JcError("002", "Error 2")
            };

            var result = JcResult.Failed(errors);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Errors.Count());
            for (int i = 0; i < errors.Count(); i++)
                Assert.AreEqual(errors[i], result.Errors.ElementAt(i));
        }

        [TestMethod]
        public void ToString_On_Success_Returns_Correctly()
        {
            var expected = "Succeeded";

            var result = JcResult.Success;

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ToString_On_Failed_No_Errors_Returns_Correctly()
        {
            var expected = "Failed";

            var result = JcResult.Failed();

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ToString_On_Failed_One_Error_Returns_Correctly()
        {
            var error = new JcError("001", "Error 1");
            var expected = $"Failed: {error.Code}";

            var result = JcResult.Failed(error);

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ToString_On_Failed_Two_Errors_Returns_Correctly()
        {
            var errors = new JcError[]
            {
                new JcError("001", "Error 1"),
                new JcError("002", "Error 2")
            };
            var expected = $"Failed: {errors[0].Code},{errors[1].Code}";

            var result = JcResult.Failed(errors);

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ToString_On_Failed_Three_Errors_Returns_Correctly()
        {
            var errors = new JcError[]
            {
                new JcError("001", "Error 1"),
                new JcError("002", "Error 2"),
                new JcError("003", "Error 3")
            };
            var expected = $"Failed: {errors[0].Code},{errors[1].Code},{errors[2].Code}";

            var result = JcResult.Failed(errors);

            Assert.AreEqual(expected, result.ToString());
        }
    }
}
