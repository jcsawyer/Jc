using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class MultiTenancyBuilderExtensionTests
    {
        private readonly IServiceCollection _services;

        private readonly MultiTenancyBuilder _sut;

        public MultiTenancyBuilderExtensionTests()
        {
            _services = new ServiceCollection();

            _sut = new MultiTenancyBuilder(typeof(ITenant), _services);
        }

        [TestMethod]
        public void AddHostResolver_Adds_Resolver_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver();

            var resolver = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantResolver<ITenant>));

            Assert.IsNotNull(resolver);
            Assert.AreEqual(ServiceLifetime.Scoped, resolver.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddHostResolver_Adds_HttpContextAccessor_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver();

            var contextAccessor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IHttpContextAccessor));

            Assert.IsNotNull(contextAccessor);
            Assert.AreEqual(ServiceLifetime.Singleton, contextAccessor.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddHostResolver_Adds_MemoryCache_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver();

            var memoryCache = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IMemoryCache));

            Assert.IsNotNull(memoryCache);
            Assert.AreEqual(ServiceLifetime.Singleton, memoryCache.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddHostResolver_Adds_Generic_Host_Resolver_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver<ITenantResolver<ITenant>>();

            var resolver = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantResolver<ITenant>));

            Assert.IsNotNull(resolver);
            Assert.AreEqual(ServiceLifetime.Scoped, resolver.Lifetime);
            Assert.AreSame(_sut, builder);
        }
        [TestMethod]
        public void AddHostResolver_Generic_Adds_HttpContextAccessor_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver<ITenantResolver<ITenant>>();

            var contextAccessor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IHttpContextAccessor));

            Assert.IsNotNull(contextAccessor);
            Assert.AreEqual(ServiceLifetime.Singleton, contextAccessor.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddHostResolver_Generic_Adds_MemoryCache_To_ServiceCollection()
        {
            var builder = _sut.AddHostResolver<HostTenantResolver<ITenant>>();

            var memoryCache = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IMemoryCache));

            Assert.IsNotNull(memoryCache);
            Assert.AreEqual(ServiceLifetime.Singleton, memoryCache.Lifetime);
            Assert.AreSame(_sut, builder);
        }
    }
}
