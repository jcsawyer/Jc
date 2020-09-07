using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Caching
{
    /// <summary>
    /// A base tenant resolver that uses <see cref="IMemoryCache"/> to cache tenants
    /// to limit access to a persistent store
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant</typeparam>
    public abstract class MemoryCacheTenantResolver<TTenant> : ITenantResolver<TTenant>
        where TTenant : class, ITenant
    {
        protected readonly IMemoryCache _cache;
        protected readonly ILogger<MemoryCacheTenantResolver<TTenant>> _logger;
        protected readonly MemoryCacheTenantResolverOptions _options;

        /// <summary>
        /// Initializes a <see cref="MemoryCacheTenantResolver{TTenant}"/> using the
        /// given <paramref name="cache"/>, <paramref name="logger"/> and 
        /// <paramref name="options"/>
        /// </summary>
        /// <param name="cache"><see cref="IMemoryCache"/> cache</param>
        /// <param name="logger"><see cref="ILogger{MemoryCacheTenantResolver{TTenant}}"/> logger</param>
        /// <param name="options"><see cref="MemoryCacheTenantResolverOptions"/> options</param>
        public MemoryCacheTenantResolver(
            [NotNull]IMemoryCache cache,
            [NotNull]ILogger<MemoryCacheTenantResolver<TTenant>> logger,
            IOptions<MemoryCacheTenantResolverOptions> options = null)
        {
            _cache = cache;
            _logger = logger;
            _options = options?.Value ?? new MemoryCacheTenantResolverOptions();
        }

        /// <summary>
        /// Gets the identifying string for a tenant
        /// </summary>
        /// <returns>Tenant identifier</returns>
        protected abstract string GetTenantIdentifier();

        /// <summary>
        /// Gets additional identifiers for a tenant
        /// </summary>
        /// <param name="tenant">The tenant</param>
        /// <returns><see cref="IEnumerable{string}"/> tenant identifiers</returns>
        protected abstract IEnumerable<string> GetTenantIdentifiers(TTenant tenant);

        /// <inheritdoc cref="ITenantResolver{TTenant}.ResolveAsync(CancellationToken)"/>
        protected abstract Task<TTenant> ResolveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates <see cref="MemoryCacheEntryOptions"/> with a default sliding
        /// expiration of 1 hour. This can be overriden to use different options
        /// </summary>
        /// <returns><see cref="MemoryCacheEntryOptions"/> cache entry options</returns>
        protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions()
            => new MemoryCacheEntryOptions().SetSlidingExpiration(new TimeSpan(1, 0, 0));

        /// <summary>
        /// Attempts to resovle the <typeparamref name="TTenant"/> from memory cache.
        /// If not found, it will attempt to resolve via the implemented resolver and
        /// add to the cache if found
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token</param>
        /// <returns><typeparamref name="TTenant"/> the tenant or null</returns>
        async Task<TTenant> ITenantResolver<TTenant>.ResolveAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cacheKey = GetTenantIdentifier();
            if (string.IsNullOrEmpty(cacheKey))
                return null;

            var tenant = _cache.Get<TTenant>(cacheKey);
            if (tenant == null)
            {
                _logger.LogDebug($"Tenant not found in cache with key \"{cacheKey}\".{Environment.NewLine}\tAttempting to resolve");
                tenant = await ResolveAsync(cancellationToken);

                if (tenant != null)
                {
                    var tenantIdentifiers = GetTenantIdentifiers(tenant);
                    if (tenantIdentifiers != null)
                    {
                        var cacheEntryOptions = GetCacheEntryOptions();

                        _logger.LogDebug($"Tenant \"{tenant.Name}\" resolved. Caching with keys \"{string.Join(",", tenantIdentifiers.ToList())}\"");
                        foreach (var identifier in tenantIdentifiers)
                            _cache.Set(identifier, tenant, cacheEntryOptions);
                    }
                }
            }
            else
            {
                _logger.LogDebug($"Tenant \"{tenant.Name}\" found in cache with key \"{cacheKey}\"");
            }

            return tenant;
        }

        /// <summary>
        /// Builds and retrieves the <see cref="MemoryCacheEntryOptions"/> for controlling
        /// cache entry expiration
        /// </summary>
        /// <returns><see cref="MemoryCacheEntryOptions"/> cache options</returns>
        private MemoryCacheEntryOptions GetCacheEntryOptions()
        {
            var cacheEntryOptions = CreateCacheEntryOptions();

            if (_options.EvictAllEntriesOnExpiry)
            {
                var tokenSource = new CancellationTokenSource();
                cacheEntryOptions
                    .RegisterPostEvictionCallback((key, value, reason, state) => tokenSource.Cancel())
                    .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            return cacheEntryOptions;
        }
    }
}
