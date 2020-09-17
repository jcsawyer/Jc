using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class TenantContextTests
    {
        private readonly Mock<ITenant> _tenant;

        public TenantContextTests()
        {
            _tenant = new Mock<ITenant>();
        }

        [TestMethod]
        public void Constructor_Throws_When_Tenant_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantContext<ITenant>(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("tenant", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Sets_Guid()
        {
            var result = new TenantContext<ITenant>(_tenant.Object);

            Assert.IsNotNull(result.Id);
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod]
        public void Constructor_Sets_Unique_Guid()
        {
            var result1 = new TenantContext<ITenant>(_tenant.Object);
            var result2 = new TenantContext<ITenant>(_tenant.Object);

            Assert.AreNotEqual(result1.Id, result2.Id);
        }

        [TestMethod]
        public void Constructor_Sets_Tenant()
        {
            var result = new TenantContext<ITenant>(_tenant.Object);

            Assert.IsNotNull(result.Tenant);
            Assert.AreSame(_tenant.Object, result.Tenant);
        }
    }
}
