using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Caching.Tests.Mocks
{
    public class MockMemoryCacheTenantResolver : MemoryCacheTenantResolver<ITenant>
    {
        private readonly Mock<ITenant> _tenant;

        public MockMemoryCacheTenantResolver(
            IMemoryCache cache,
            ILogger<MemoryCacheTenantResolver<ITenant>> logger,
            IOptions<MemoryCacheTenantResolverOptions> options,
            Mock<ITenant> tenant) : base(cache, logger, options)
        {
            _tenant = tenant;
        }

        protected override string GetTenantIdentifier()
            => _tenant.Object.Name;

        protected override IEnumerable<string> GetTenantIdentifiers(ITenant tenant)
        {
            yield return _tenant.Object.Name;
            yield return _tenant.Object.Id.ToString();
        }

        protected override Task<ITenant> ResolveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_tenant.Object);
    }
}
