using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Jc.MultiTenancy.AspNetCore.Tests
{
    [TestClass]
    public class ApplicationBuilderExtensionTests
    {
        private readonly IApplicationBuilder _builder;
        private readonly Mock<IServiceProvider> _serviceProvider;

        public ApplicationBuilderExtensionTests()
        {
            _serviceProvider = new Mock<IServiceProvider>();

            _builder = new ApplicationBuilder(_serviceProvider.Object);
        }

        [TestMethod]
        public void UseMultiTenancy_Adds_Middleware()
        {
            _builder.UseMultiTenancy<ITenant>();
        }

        [TestMethod]
        public void UsePerTenant_Adds_Middleware()
        {
            _builder.UsePerTenant<ITenant>((context, app) =>
            {
            });
        }
    }
}
