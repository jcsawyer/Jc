using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jc.MultiTenancy.Tests
{
    [TestClass]
    public class MultiTenancyOptionsTests
    {
        [TestMethod]
        public void Constructor_Sets_Default_Unresolved_Options()
        {
            var options = new MultiTenancyOptions();
            Assert.IsNotNull(options.Unresolved);
            Assert.AreEqual(false, options.Unresolved.IsPermanentRedirect);
            Assert.AreEqual(string.Empty, options.Unresolved.RedirectUrl);
            Assert.AreEqual(string.Empty, options.Unresolved.InactiveRedirectUrl);
        }
    }
}
