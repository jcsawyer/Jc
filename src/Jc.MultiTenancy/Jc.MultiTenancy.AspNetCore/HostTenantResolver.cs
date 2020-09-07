using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Jc.MultiTenancy.Caching;
using System.Threading;

namespace Jc.MultiTenancy.AspNetCore
{
    public class HostTenantResolver<TTenant> : MemoryCacheTenantResolver<TTenant>
        where TTenant : class, ITenant
    {
        private readonly ITenantStore<TTenant> _store;
        private readonly HttpContext _context;

        /// <summary>
        /// Initializes a <see cref="HostTenantResolver{TTenant}"/> using the specified
        /// <paramref name="cache"/>, <paramref name="options"/>, <paramref name="store"/>
        /// that pulls the host header from the <paramref name="httpAccessor"/> and logs to
        /// the given <paramref name="logger"/>
        /// </summary>
        /// <param name="cache"><see cref="IMemoryCache"/> cache</param>
        /// <param name="options"><see cref="IOptions{MemoryCacheTenantResolverOptions}"/> options</param>
        /// <param name="logger"><see cref="ILogger{HostTenantResolver{TTenant}}"/> logger</param>
        /// <param name="store"><see cref="ITenantStore{TTenant}"/> store</param>
        /// <param name="httpAccessor"><see cref="IHttpContextAccessor"/> context accessor</param>
        public HostTenantResolver(
            [NotNull] IMemoryCache cache,
            [NotNull] IOptions<MemoryCacheTenantResolverOptions> options,
            [NotNull] ILogger<HostTenantResolver<TTenant>> logger,
            [NotNull] ITenantStore<TTenant> store,
            [NotNull] IHttpContextAccessor httpAccessor) : base(cache, logger, options)
        {
            _store = store;
            _context = httpAccessor.HttpContext;
        }

        /// <summary>
        /// Gets the identifying string for a tenant from the <see cref="HttpContext"/>
        /// host header
        /// </summary>
        /// <return>Http request host header</returns>
        protected override string GetTenantIdentifier()
            => _context.Request.Host.Value.ToLower();

        /// <inheritdoc/>
        protected override IEnumerable<string> GetTenantIdentifiers(TTenant tenant)
        {
            yield return tenant.Name;
            yield return tenant.Id.ToString();
        }

        /// <inheritdoc/>
        public override async Task<TTenant> ResolveAsync(CancellationToken cancellationToken = default)
        {
            var tenantIdentifier = GetTenantIdentifier();
            _logger.LogDebug($"Attempting to resolve tenant with host \"{tenantIdentifier}\"");
            cancellationToken.ThrowIfCancellationRequested();
            return await _store.FindByHostAsync(tenantIdentifier) ?? await _store.FindByNameAsync(tenantIdentifier);
        }

        /// <inheritdoc/>
        protected override MemoryCacheEntryOptions CreateCacheEntryOptions()
            => base.CreateCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
    }
}
