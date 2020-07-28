using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.AspNetCore.Middleware
{
    /// <summary>
    /// Per-tenant application request pipeline delegate
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantPipelineMiddleware<TTenant>
        where TTenant : class, ITenant
    {
        private readonly RequestDelegate _next;
        private readonly IApplicationBuilder _app;
        private readonly Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> _configuration;

        private readonly ConcurrentDictionary<TTenant, Lazy<RequestDelegate>> pipelines = new ConcurrentDictionary<TTenant, Lazy<RequestDelegate>>();

        /// <summary>
        /// Initializes a <see cref="TenantPipelineMiddleware{TTenant}"/> for multi tenancy
        /// per-tenant application pipelines
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the request pipieline</param>
        /// <param name="app"><see cref="IApplicationBuilder"/> app builder</param>
        /// <param name="configuration"><see cref="Action{TenantPipelineBuilderContext{TTenant}, {IApplicationBuilder}}"/> configuration action</param>
        public TenantPipelineMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IApplicationBuilder app,
            [NotNull] Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
        {
            _next = next;
            _app = app;
            _configuration = configuration;
        }

        /// <summary>
        /// Executes in the request pipeline
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task Invoke([NotNull] HttpContext context)
        {
            var tenantContext = context.GetTenantContext<TTenant>();
            if (tenantContext != null)
            {
                var pipeline = pipelines.GetOrAdd(
                    tenantContext.Tenant,
                    new Lazy<RequestDelegate>(() => BuildTenantPipeline(tenantContext)));

                await pipeline.Value(context);
            }
        }

        /// <summary>
        /// Builds a per-tenant application pipeline
        /// </summary>
        /// <param name="tenantContext"><see cref="TenantContext{TTenant}"/> context</param>
        /// <returns><see cref="RequestDelegate"/> used by application to handle HTTP requests</returns>
        private RequestDelegate BuildTenantPipeline([NotNull] TenantContext<TTenant> tenantContext)
        {
            var builder = _app.New();
            var context = new TenantPipelineBuilderContext<TTenant>
            {
                TenantContext = tenantContext,
                Tenant = tenantContext.Tenant
            };

            _configuration(context, builder);

            builder.Run(_next);

            return builder.Build();
        }
    }
}
