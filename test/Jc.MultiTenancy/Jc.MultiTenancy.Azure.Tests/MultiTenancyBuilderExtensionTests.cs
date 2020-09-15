using Azure.Storage.Blobs;
using Jc.MultiTenancy.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Jc.MultiTenancy.Azure.Tests
{
    [TestClass]
    public class MultiTenancyBuilderExtensionTests
    {
        private readonly Type _tenantType;
        private readonly IServiceCollection _services;

        private readonly MultiTenancyBuilder _sut;

        public MultiTenancyBuilderExtensionTests()
        {
            _tenantType = typeof(Tenant);
            _services = new ServiceCollection();

            _sut = new MultiTenancyBuilder(_tenantType, _services);
        }

        [TestMethod]
        public void AddAzureBlobStore_Adds_TenantStore_To_ServiceCollection()
        {
            var builder = _sut.AddAzureBlobStore();

            var store = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore<Tenant>));

            Assert.IsNotNull(store);
            Assert.AreEqual(ServiceLifetime.Scoped, store.Lifetime);
            Assert.AreSame(_sut, builder);
        }

        [TestMethod]
        public void AddAzureBlobStore_Adds_BlobServiceClient_To_ServiceCollection()
        {
            var mockOptions = new BlobTenantStoreOptions
            {
                BlobName = "Test Blob",
                ContainerName = "Container Name",
                ConnectionString = "UseDevelopmentStorage=true"
            };
            var builder = _sut.AddAzureBlobStore((options) => {
                options.BlobName = mockOptions.BlobName;
                options.ContainerName = mockOptions.ContainerName;
                options.ConnectionString = mockOptions.ConnectionString;
            });

            using (var scope = _services.BuildServiceProvider().CreateScope())
            {
                var services = scope.ServiceProvider;
                var client = services.GetRequiredService<BlobServiceClient>();

                Assert.IsNotNull(client);
            }
        }

        [TestMethod]
        public void AddAzureBlobStore_Adds_BlobTenantStoreOptions_To_ServiceCollection()
        {
            var mockOptions = new BlobTenantStoreOptions 
            { 
                BlobName = "Test Blob",
                ContainerName = "Container Name",
                ConnectionString = "Connection String" 
            };
            var builder = _sut.AddAzureBlobStore((options) => {
                options.BlobName = mockOptions.BlobName;
                options.ContainerName = mockOptions.ContainerName;
                options.ConnectionString = mockOptions.ConnectionString;
            });

            using (var scope = _services.BuildServiceProvider().CreateScope())
            {
                var services = scope.ServiceProvider;
                var options = services.GetRequiredService<BlobTenantStoreOptions>();

                Assert.IsNotNull(options);
                Assert.AreEqual(mockOptions.BlobName, options.BlobName);
                Assert.AreEqual(mockOptions.ContainerName, options.ContainerName);
                Assert.AreEqual(mockOptions.ConnectionString, options.ConnectionString);
            }
        }
    }
}
