using Jc.MultiTenancy.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class HostTenantResolverTests
    {
        private readonly ITenantResolver<ITenant> _sut;

        private readonly Mock<ITenantStore<ITenant>> _store;
        private readonly Mock<IHttpContextAccessor> _contextAccessor;
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ILogger<HostTenantResolver<ITenant>>> _logger;
        private readonly Mock<IOptions<MemoryCacheTenantResolverOptions>> _options;
        private readonly Mock<ITenant> _tenant;
        private readonly DefaultHttpContext _context;

        public HostTenantResolverTests()
        {
            _store = new Mock<ITenantStore<ITenant>>();
            _contextAccessor = new Mock<IHttpContextAccessor>();
            _context = new DefaultHttpContext();
            _contextAccessor.SetupGet(x => x.HttpContext).Returns(_context);
            _cache = new Mock<IMemoryCache>();
            _logger = new Mock<ILogger<HostTenantResolver<ITenant>>>();
            _options = new Mock<IOptions<MemoryCacheTenantResolverOptions>>();
            _tenant = new Mock<ITenant>();

            _sut = new HostTenantResolver<ITenant>(_cache.Object, _options.Object, _logger.Object, _store.Object, _contextAccessor.Object);
        }

        [TestMethod]
        public async Task ResolveAsync_Returns_Tenant_From_Store_By_Host()
        {
            string host = "testtenant.jc";
            object tenant = null;

            var cacheEntry = Mock.Of<ICacheEntry>();
            Mock.Get(cacheEntry).SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            Mock.Get(cacheEntry).SetupGet(c => c.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            _cache.Setup(x => x.TryGetValue(host, out tenant)).Returns(true);
            _cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            _tenant.SetupGet(x => x.Host).Returns(host);
            _store.Setup(x => x.FindByHostAsync(host, It.IsAny<CancellationToken>())).ReturnsAsync(_tenant.Object).Verifiable();

            _context.Request.Host = new HostString(host);

            var result = await _sut.ResolveAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(host, result.Host);
            _store.Verify();
        }

        [TestMethod]
        public async Task ResolveAsync_Returns_Tenant_From_Store_By_Name_If_No_Matching_Host()
        {
            string host = "testtenant.jc";
            object tenant = null;

            var cacheEntry = Mock.Of<ICacheEntry>();
            Mock.Get(cacheEntry).SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            Mock.Get(cacheEntry).SetupGet(c => c.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            _cache.Setup(x => x.TryGetValue(host, out tenant)).Returns(true);
            _cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            _tenant.SetupGet(x => x.Name).Returns(host);
            _store.Setup(x => x.FindByHostAsync(host, It.IsAny<CancellationToken>())).Returns(Task.FromResult<ITenant>(null)).Verifiable();
            _store.Setup(x => x.FindByNameAsync(host, It.IsAny<CancellationToken>())).ReturnsAsync(_tenant.Object).Verifiable();

            _context.Request.Host = new HostString(host);

            var result = await _sut.ResolveAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(host, result.Name);
            _store.Verify();
        }

        [TestMethod]
        public async Task ResolveAsync_Returns_Null_If_Not_In_Store()
        {
            string host = "testtenant.jc";
            object tenant = null;

            var cacheEntry = Mock.Of<ICacheEntry>();
            Mock.Get(cacheEntry).SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            Mock.Get(cacheEntry).SetupGet(c => c.PostEvictionCallbacks).Returns(new List<PostEvictionCallbackRegistration>());

            _cache.Setup(x => x.TryGetValue(host, out tenant)).Returns(true);
            _cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            _store.Setup(x => x.FindByHostAsync(host, It.IsAny<CancellationToken>())).Returns(Task.FromResult<ITenant>(null)).Verifiable();
            _store.Setup(x => x.FindByNameAsync(host, It.IsAny<CancellationToken>())).Returns(Task.FromResult<ITenant>(null)).Verifiable();

            _context.Request.Host = new HostString(host);

            var result = await _sut.ResolveAsync();

            Assert.IsNull(result);
            _store.Verify();
        }
    }
}
