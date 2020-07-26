using System;
using System.Diagnostics.CodeAnalysis;

using Jc.MultiTenancy;
using Jc.MultiTenancy.AspNetCore;
using Jc.MultiTenancy.AspNetCore.Middleware;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> multi tenancy extensions
    /// </summary>
    public static class MultiTenancyApplicationBuilderExtensions
    {
        /// <summary>
        /// Add per-tenant application middleware through the
        /// <see cref="TenantPipelineBuilderContext{TTenant}"/>
        /// </summary>
        /// <typeparam name="TTenant">Type of tenant</typeparam>
        /// <param name="configuration"><see cref="Action{TenantPipelineBuilderContext{TTenant}, IApplicationBuilder}"/> configuration action</param>
        /// <returns><see cref="IApplicationBuilder"/> builder</returns>
        public static IApplicationBuilder UsePerTenant<TTenant>(
            this IApplicationBuilder app,
            [NotNull] Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
            where TTenant : class, ITenant
        {
            app.Use(next => new TenantPipelineMiddleware<TTenant>(next, app, configuration).Invoke);
            return app;
        }
    }
}
