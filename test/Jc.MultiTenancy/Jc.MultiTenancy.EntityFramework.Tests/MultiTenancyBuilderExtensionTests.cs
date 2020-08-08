using System;
using System.Linq;
using Jc.MultiTenancy.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jc.MultiTenancy.EntityFramework.Tests
{
    [TestClass]
    public class MultiTenancyBuilderExtensionTests
    {
        private readonly Type _tenantType;
        private readonly IServiceCollection _services;

        private readonly MultiTenancyBuilder _sut;

        public MultiTenancyBuilderExtensionTests()
        {
            _tenantType = typeof(Tenant);
            _services = new ServiceCollection();

            _sut = new MultiTenancyBuilder(_tenantType, _services);
        }

        [TestMethod]
        public void AddEntityFrameworkStore_Adds_TenantDbContext_To_ServiceCollection()
        {
            var builder = _sut.AddEntityFrameworkStore();

            var context = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(TenantDbContext));

            Assert.IsNotNull(context);
            Assert.AreEqual(ServiceLifetime.Scoped, context.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddEntityFrameworkStore_Adds_TenantStore_To_ServiceCollection()
        {
            var builder = _sut.AddEntityFrameworkStore();

            var store = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore<Tenant>));

            Assert.IsNotNull(store);
            Assert.AreEqual(ServiceLifetime.Scoped, store.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddEntityFrameworkStore_Adds_TenantStore_With_Context_To_ServiceCollection()
        {
            var builder = _sut.AddEntityFrameworkStore();

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var store = scope.ServiceProvider.GetService<ITenantStore<Tenant>>();
                var genericParams = store.GetType().GetGenericArguments();
                Assert.AreEqual(_tenantType, genericParams[0]);
                Assert.AreEqual(typeof(TenantDbContext), genericParams[1]);
            }
        }

        [TestMethod]
        public void AddEntityFrameworkStore_Adds_Generic_Type_DbContext_To_ServiceCollection()
        {
            var builder = _sut.AddEntityFrameworkStore<DbContext>();

            var context = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(DbContext));

            Assert.IsNotNull(context);
            Assert.AreEqual(ServiceLifetime.Scoped, context.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddEntityFrameworkStore_Adds_Generic_Context_TenantStore_To_ServiceCollection()
        {
            var builder = _sut.AddEntityFrameworkStore<TenantDbContext>();

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var store = scope.ServiceProvider.GetService<ITenantStore<Tenant>>();
                var genericParams = store.GetType().GetGenericArguments();
                Assert.AreEqual(_tenantType, genericParams[0]);
                Assert.AreEqual(typeof(TenantDbContext), genericParams[1]);
            }
        }
    }
}
