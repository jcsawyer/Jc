using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Jc.MultiTenancy.Extensions.Tests
{
    [TestClass]
    public class MultiTenancyBuilderTests
    {
        private readonly Type _tenantType;
        private readonly IServiceCollection _services;

        private readonly MultiTenancyBuilder _sut;

        public MultiTenancyBuilderTests()
        {
            _tenantType = typeof(ITenant);
            _services = new ServiceCollection();

            _sut = new MultiTenancyBuilder(_tenantType, _services);
        }

        [TestMethod]
        public void Constructor_Sets_TenantType_And_Services()
        {
            var tenantType = typeof(ITenant);

            var builder = new MultiTenancyBuilder(tenantType, _services);

            Assert.IsNotNull(builder);
            Assert.AreEqual(tenantType, builder.TenantType);
            Assert.AreEqual(_services, builder.Services);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_TenantType_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new MultiTenancyBuilder(null, _services));

            Assert.AreEqual("tenantType", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_Services_Is_Null()
        {
            var tenantType = typeof(ITenant);

            var exception = Assert.ThrowsException<ArgumentNullException>(() => new MultiTenancyBuilder(tenantType, null));

            Assert.AreEqual("services", exception.ParamName);
        }

        [TestMethod]
        public void AddStore_Adds_A_Scoped_Tenant_Store_To_ServiceCollection()
        {
            var builder = _sut.AddStore<ITenantStore<ITenant>>();

            var tenantStore = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore<>));
            
            Assert.IsNotNull(tenantStore);
            Assert.AreEqual(ServiceLifetime.Scoped, tenantStore.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddResolver_Adds_A_Scoped_Tenant_Resolver_To_ServiceCollection()
        {
            var builder = _sut.AddResolver<ITenantResolver<ITenant>>();

            var tenantResolver = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantResolver<>));

            Assert.IsNotNull(tenantResolver);
            Assert.AreEqual(ServiceLifetime.Scoped, tenantResolver.Lifetime);
            Assert.AreSame(_sut, builder);
        }
    }
}
