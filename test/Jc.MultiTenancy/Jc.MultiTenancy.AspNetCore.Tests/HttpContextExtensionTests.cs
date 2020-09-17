using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class HttpContextExtensionTests
    {
        private const string TenantKey = "Jc.Tenant";

        private readonly Mock<ITenant> _tenant;

        public HttpContextExtensionTests()
        {
            _tenant = new Mock<ITenant>();
        }

        [TestMethod]
        public void SetTenant_Sets_HttpContext_Item()
        {
            var tenantId = Guid.NewGuid();
            _tenant.SetupGet(x => x.Id).Returns(tenantId);

            var context = new DefaultHttpContext();
            
            context.SetTenant(new TenantContext<ITenant>(_tenant.Object));

            Assert.IsNotNull(context.Items[TenantKey]);
            var contextItem = (TenantContext<ITenant>)context.Items[TenantKey];
            Assert.IsNotNull(contextItem.Id);
            Assert.AreEqual(tenantId, contextItem.Tenant.Id);
        }

        [TestMethod]
        public void GetTenantContext_Returns_TenantContext_From_HttpContext()
        {
            var context = new DefaultHttpContext();
            context.Items.Add(TenantKey, new TenantContext<ITenant>(_tenant.Object));

            var result = context.GetTenantContext<ITenant>();

            Assert.IsNotNull(result);
            Assert.AreSame(_tenant.Object, result.Tenant);
        }

        [TestMethod]
        public void GetTenantContext_Returns_Null_If_No_TenantContext_In_HttpContext()
        {
            var context = new DefaultHttpContext();
            var result = context.GetTenantContext<ITenant>();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetTenant_Returns_Tenant_From_Context()
        {
            var context = new DefaultHttpContext();
            context.Items.Add(TenantKey, new TenantContext<ITenant>(_tenant.Object));

            var result = context.GetTenant<ITenant>();

            Assert.IsNotNull(result);
            Assert.AreSame(_tenant.Object, result);
        }

        [TestMethod]
        public void GetTenant_Returns_Null_If_No_Tenant_In_Context()
        {
            var context = new DefaultHttpContext();

            var result = context.GetTenant<ITenant>();

            Assert.IsNull(result);
        }
    }
}
