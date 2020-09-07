using Jc.MultiTenancy.Caching.Tests.Mocks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Caching.Tests
{
    [TestClass]
    public class MemoryCacheTenantResolverTests
    {
        private readonly ITenantResolver<ITenant> _sut;

        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ILogger<MemoryCacheTenantResolver<ITenant>>> _logger;
        private readonly Mock<IOptions<MemoryCacheTenantResolverOptions>> _options;
        private readonly Mock<ITenant> _tenant;

        public MemoryCacheTenantResolverTests()
        {
            _cache = new Mock<IMemoryCache>();
            _logger = new Mock<ILogger<MemoryCacheTenantResolver<ITenant>>>();
            _options = new Mock<IOptions<MemoryCacheTenantResolverOptions>>();
            _tenant = new Mock<ITenant>();

            _sut = new MockMemoryCacheTenantResolver(_cache.Object, _logger.Object, _options.Object, _tenant);
        }


        [TestMethod]
        public async Task ResolveAsync_Returns_Null_When_TenantIdentifier_Returns_Null_Or_Empty()
        {
            _tenant.SetupGet(x => x.Name).Returns("");

            var result = await _sut.ResolveAsync();

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ResolveAsync_Returns_From_Cache_When_It_Exists()
        {
            string tenantIdentifier = "Test Tenant";
            object tenant = _tenant.Object;

            _tenant.SetupGet(x => x.Name).Returns(tenantIdentifier);
            _cache.Setup(x => x.TryGetValue(tenantIdentifier, out tenant)).Returns(true).Verifiable();

            var result = await _sut.ResolveAsync();

            _cache.VerifyAll();
        }

        [TestMethod]
        public async Task ResolveAsync_Calls_Implementation_When_It_Does_Not_Exist_In_Cache()
        {
            string tenantIdentifier = "Test Tenant";
            object tenant = null;

            var cacheEntry = Mock.Of<ICacheEntry>();
            Mock.Get(cacheEntry).SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            Mock.Get(cacheEntry).SetupGet(c => c.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            _tenant.SetupGet(x => x.Name).Returns(tenantIdentifier);
            _cache.Setup(x => x.TryGetValue(tenantIdentifier, out tenant)).Returns(true).Verifiable();
            _cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            var result = await _sut.ResolveAsync();

            _cache.VerifyAll();
            Assert.AreEqual(_tenant.Object, result);
        }

        [TestMethod]
        public async Task ResolveAsync_Adds_Identifiers_To_Cache_When_Does_Not_Exist_In_Cache()
        {
            string tenantIdentifier = "Test Tenant";
            object tenant = null;

            var cacheEntry = Mock.Of<ICacheEntry>();
            Mock.Get(cacheEntry).SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            Mock.Get(cacheEntry).SetupGet(c => c.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            _tenant.SetupGet(x => x.Name).Returns(tenantIdentifier);
            _cache.Setup(x => x.TryGetValue(tenantIdentifier, out tenant)).Returns(true).Verifiable();
            _cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntry).Verifiable();

            var result = await _sut.ResolveAsync();

            _cache.VerifyAll();
        }

        [TestMethod]
        public async Task ResolveAsync_Throws_When_Cancellation_Requested()
        {
            var cancellationToken = new CancellationTokenSource();
            cancellationToken.Cancel();
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => _sut.ResolveAsync(cancellationToken.Token));
        }
    }
}
