using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class PipelineBuilderContextTests
    {
        private readonly Mock<ITenant> _tenant;

        public PipelineBuilderContextTests()
        {
            _tenant = new Mock<ITenant>();
        }

        [TestMethod]
        public void TenantContext_Is_Set()
        {
            var pipelineBuilderContext = new TenantPipelineBuilderContext<ITenant>();
            var tenantContext = new TenantContext<ITenant>(_tenant.Object);
            pipelineBuilderContext.TenantContext = tenantContext;

            Assert.AreEqual(tenantContext, pipelineBuilderContext.TenantContext);
        }

        [TestMethod]
        public void Tenant_Is_Set()
        {
            var pipelineBuilderContext = new TenantPipelineBuilderContext<ITenant>();
            pipelineBuilderContext.Tenant = _tenant.Object;

            Assert.AreEqual(_tenant.Object, pipelineBuilderContext.Tenant);
        }
    }
}
