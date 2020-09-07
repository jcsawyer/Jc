using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jc.MultiTenancy.Caching.Tests
{
    [TestClass]
    public class MemoryCacheTenantResolverOptionsTests
    {
        [TestMethod]
        public void Constructor_Sets_Default_MemoryCache_Tenant_Resolver_Options()
        {
            var options = new MemoryCacheTenantResolverOptions();
            Assert.IsTrue(options.EvictAllEntriesOnExpiry);
        }
    }
}
