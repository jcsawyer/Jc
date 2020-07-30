using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Jc.MultiTenancy.Extensions.Tests
{
    [TestClass]
    public class MultiTenancyServiceCollectionExtensionTests
    {
        [TestMethod]
        public void AddMultiTenancy_No_Options_Adds_TenantMananger_To_Services()
        {
            var services = new ServiceCollection();
            var builder = services.AddMultiTenancy<ITenant>();

            var manager = services.FirstOrDefault(x => x.ServiceType == typeof(TenantManager<ITenant>));
            
            Assert.IsNotNull(builder);
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void AddMultiTenancy_No_Options_Adds_Default_Options()
        {
            var expected = new MultiTenancyOptions();
            var services = new ServiceCollection();

            var builder = services.AddMultiTenancy<ITenant>();

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<MultiTenancyOptions>>();
            
            Assert.IsNotNull(builder);

            Assert.AreEqual(expected.Unresolved.RedirectUrl, options.Value.Unresolved.RedirectUrl);
            Assert.AreEqual(expected.Unresolved.IsPermanentRedirect, options.Value.Unresolved.IsPermanentRedirect);
            Assert.AreEqual(expected.Unresolved.InactiveRedirectUrl, options.Value.Unresolved.InactiveRedirectUrl);
        }

        [TestMethod]
        public void AddMultiTenant_With_Options_Configures_The_Options_In_Service_Collection()
        {
            var expected = new MultiTenancyOptions()
            {
                Unresolved = new MultiTenancyUnresolvedOptions()
                {
                    IsPermanentRedirect = true,
                    RedirectUrl = "redirect-url",
                    InactiveRedirectUrl = "invactive-redirect-url"
                }
            };
            var services = new ServiceCollection();

            var builder = services.AddMultiTenancy<ITenant>(options =>
            {
                options.Unresolved.IsPermanentRedirect = expected.Unresolved.IsPermanentRedirect;
                options.Unresolved.RedirectUrl = expected.Unresolved.RedirectUrl;
                options.Unresolved.InactiveRedirectUrl = expected.Unresolved.InactiveRedirectUrl;
            });

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<MultiTenancyOptions>>();

            Assert.IsNotNull(builder);

            Assert.AreEqual(expected.Unresolved.RedirectUrl, options.Value.Unresolved.RedirectUrl);
            Assert.AreEqual(expected.Unresolved.IsPermanentRedirect, options.Value.Unresolved.IsPermanentRedirect);
            Assert.AreEqual(expected.Unresolved.InactiveRedirectUrl, options.Value.Unresolved.InactiveRedirectUrl);
        }
    }
}
