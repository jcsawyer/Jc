using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.AspNetCore.Middleware
{
    /// <summary>
    /// Tenant resolving request pipeline delegate
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantResolverMiddleware<TTenant>
        where TTenant : class, ITenant
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolverMiddleware<TTenant>> _logger;

        /// <summary>
        /// Initializes a <see cref="TenantPipelineMiddleware{TTenant}"/> for multi tenancy
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the request pipeline</param>
        /// <param name="logger"><see cref="ILogger{TenantResolverMiddleware{TTenant}}"/> logger</param>
        public TenantResolverMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] ILogger<TenantResolverMiddleware<TTenant>> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Executes in the request pipeline
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/></param>
        /// <param name="tenantResolver"><see cref="ITenantResolver{TTenant}"/> tenant resolver</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task Invoke(
            [NotNull] HttpContext context,
            [NotNull] ITenantResolver<TTenant> tenantResolver)
        {
            _logger.LogDebug($"Resolving tenant using {tenantResolver.GetType().Name}");

            var tenant = await tenantResolver.ResolveAsync(context.RequestAborted);

            if (tenant != null)
            {
                _logger.LogDebug($"Tenant \"{tenant.Name}\" resolved{Environment.NewLine}\tAdding to HttpContext");
                context.SetTenant<TTenant>(new TenantContext<TTenant>(tenant));
            }
            else
            {
                _logger.LogDebug("Tenant could not be resolved");
            }

            await _next.Invoke(context);
        }
    }
}
