using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.AspNetCore.Middleware
{
    /// <summary>
    /// Unresolved tenant request pipeline delegate
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantUnresolvedMiddleware<TTenant>
        where TTenant : class, ITenant
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantUnresolvedMiddleware<TTenant>> _logger;
        private readonly MultiTenancyUnresolvedOptions _options;

        /// <summary>
        /// Initializes a <see cref="TenantUnresolvedMiddleware{TTenant}"/> for 
        /// handling requests with no tenant context
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the request pipeline</param>
        /// <param name="logger"><see cref="ILogger{TenantUnresolvedMiddleware{TTenant}}"/> logger</param>
        /// <param name="options"><see cref="MultiTenancyOptions"/> options</param>
        public TenantUnresolvedMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] ILogger<TenantUnresolvedMiddleware<TTenant>> logger,
            [NotNull] IOptions<MultiTenancyOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options?.Value?.Unresolved ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(_options.RedirectUrl))
                _logger.LogWarning($"Unresolved tenant redirect url is not set. No redirect will be used");
        }

        /// <summary>
        /// Executes in the request pipeline
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task Invoke([NotNull] HttpContext context)
        {
            var tenantContext = context.GetTenantContext<TTenant>();
            if (tenantContext == null || !tenantContext.Tenant.IsActive)
            {
                string redirectUrl = _options.RedirectUrl;

                if (!tenantContext.Tenant.IsActive)
                {
                    _logger.LogInformation($"Attempt to access inactive tenant \"{tenantContext.Tenant.Name}\"");
                    if (!string.IsNullOrEmpty(_options.InactiveRedirectUrl))
                        redirectUrl = _options.InactiveRedirectUrl;
                }

                if (!string.IsNullOrEmpty(redirectUrl))
                    context.Response.Redirect(redirectUrl);
                else
                    context.Response.Clear();

                context.Response.StatusCode = _options.IsPermanentRedirect
                    ? StatusCodes.Status301MovedPermanently
                    : StatusCodes.Status302Found;
                
                return;
            }

            await _next(context);
        }
    }
}
