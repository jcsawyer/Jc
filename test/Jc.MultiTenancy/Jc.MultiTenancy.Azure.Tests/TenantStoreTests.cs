using Azure.Storage.Blobs;
using Jc.MultiTenancy.Stores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Azure.Tests
{
    [TestClass]
    public class TenantStoreTests
    {
        private readonly BlobServiceClient _service;
        private readonly BlobContainerClient _container;
        private readonly BlobTenantStoreOptions _options;

        private readonly TenantStore<Tenant> _sut;

        public TenantStoreTests()
        {
            Helpers.StorageEmulator.Clear();
            Helpers.StorageEmulator.Start();

            _options = new BlobTenantStoreOptions
            {
                BlobName = "",
                ContainerName = "container-name",
                ConnectionString = "UseDevelopmentStorage=true"
            };
            
            _service = new BlobServiceClient("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://127.0.0.1");
            _container = _service.GetBlobContainerClient(_options.ContainerName);
            _container.CreateIfNotExists();
            
            _sut = new TenantStore<Tenant>(_service, _options);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helpers.StorageEmulator.Clear();
            Helpers.StorageEmulator.Emulator.Kill();
        }

        [TestMethod]
        public void Constructor_Throws_When_Client_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore(null, _options));

            Assert.AreEqual("client", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Throws_When_Options_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore(_service, null));

            Assert.AreEqual("options", exception.ParamName);
        }

        [TestMethod]
        public void Generic_Tenant_Constructor_Throws_When_Client_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore<Tenant>(null, _options));

            Assert.AreEqual("client", exception.ParamName);
        }

        [TestMethod]
        public void Generic_Tenant_Constructor_Throws_When_Options_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore<Tenant>(_service, null));

            Assert.AreEqual("options", exception.ParamName);
        }

        [TestMethod]
        public async Task CreateAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.CreateAsync(tenant, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task CreateAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.CreateAsync(tenant));
        }

        [TestMethod]
        public async Task CreateAsync_Saves_To_Blob_Storage()
        {
            const string blobName = "CreateAsyncSaves";
            var tenant = new Tenant { Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "hostname.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.CreateAsync(tenant);
            var blobTenants = await LoadBlob(blobName);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(2, blobTenants.Count);
            Assert.IsTrue(blobTenants.Any(x => x.Name.Equals(tenant.Name)));
        }

        [TestMethod]
        public async Task CreateAsync_Fails_If_Name_Already_Exists()
        {
            const string blobName = "CreateAsyncThrowsName";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "hostname.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.CreateAsync(tenant);

            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102102", result.Errors.First().Code);
            Assert.AreEqual($"Tenant with name {tenant.Name} already exists", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task CreateAsync_Fails_If_Host_Already_Exists()
        {
            const string blobName = "CreateAsyncThrowsHost";
            var tenant = new Tenant { Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "testhost.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.CreateAsync(tenant);

            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102103", result.Errors.First().Code);
            Assert.AreEqual($"Tenant with host {tenant.Host} already exists", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task CreateAsync_Fails_If_Name_And_Host_Already_Exist()
        {
            const string blobName = "CreateAsyncThrowsName";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "testhost.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.CreateAsync(tenant);

            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102101", result.Errors.First().Code);
            Assert.AreEqual($"Tenant with name {tenant.Name} and host {tenant.Host} already exists", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task UpdateAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.UpdateAsync(tenant, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task UpdateAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.UpdateAsync(tenant));
        }

        [TestMethod]
        public async Task UpdateAsync_Saves_To_Blob_Storage()
        {
            const string blobName = "UpdateAsyncSaves";
            var tenant = new Tenant { Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            tenant.Name = "Updated Tenant";
            var result = await _sut.UpdateAsync(tenant);
            var blobTenants = await LoadBlob(blobName);

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.Errors.Any());
            Assert.AreEqual(1, blobTenants.Count);
            Assert.AreEqual(blobTenants[0].Name, tenant.Name);
        }

        [TestMethod]
        public async Task UpdateAsync_Errors_When_Tenant_Not_Found()
        {
            const string blobName = "UpdateAsyncNotFound";
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "hostname.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.UpdateAsync(tenant);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102201", result.Errors.First().Code);
            Assert.AreEqual("Tenant not found", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task UpdateAsync_Errors_When_Multiple_Tenants_Found()
        {
            const string blobName = "UpdateAsyncMultipleFound";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() 
            { 
                new Tenant { Name = "Existing Tenant", Host = "hostname.jc" },
                new Tenant { Name = "Existing Tenant", Host = "hostname.jc" }
            };
            await UploadBlob(blobName, tenants);

            var result = await _sut.UpdateAsync(tenant);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102202", result.Errors.First().Code);
            Assert.AreEqual("Multiple tenants found", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task DeleteAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.DeleteAsync(tenant, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task DeleteAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.DeleteAsync(tenant));
        }

        [TestMethod]
        public async Task DeleteAsync_Saves_To_Blob_Storage()
        {
            const string blobName = "DeleteAsyncSaves";
            var tenant = new Tenant { Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            var result = await _sut.DeleteAsync(tenant);
            var blobTenants = await LoadBlob(blobName);

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.Errors.Any());
            Assert.AreEqual(0, blobTenants.Count);
        }
        [TestMethod]
        public async Task DeleteAsync_Errors_When_Tenant_Not_Found()
        {
            const string blobName = "DeleteAsyncNotFound";
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { new Tenant { Name = "Existing Tenant", Host = "hostname.jc" } };
            await UploadBlob(blobName, tenants);

            var result = await _sut.DeleteAsync(tenant);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102301", result.Errors.First().Code);
            Assert.AreEqual("Tenant not found", result.Errors.First().Description);
        }

        [TestMethod]
        public async Task DeleteAsync_Errors_When_Multiple_Tenants_Found()
        {
            const string blobName = "DeleteAsyncMultipleFound";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>()
            {
                new Tenant { Name = "Existing Tenant", Host = "hostname.jc" },
                new Tenant { Name = "Existing Tenant", Host = "hostname.jc" }
            };
            await UploadBlob(blobName, tenants);

            var result = await _sut.DeleteAsync(tenant);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("102302", result.Errors.First().Code);
            Assert.AreEqual("Multiple tenants found", result.Errors.First().Description);
        }


        [TestMethod]
        public async Task FindByIdAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByIdAsync(Guid.NewGuid(), cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByIdAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByIdAsync(Guid.NewGuid()));
        }

        [TestMethod]
        public async Task FindByIdAsync_Finds_In_Blob_Storage()
        {
            const string blobName = "FindById";
            var id = Guid.NewGuid();
            var tenant = new Tenant { Id = id, Name = "Test Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            var result = await _sut.FindByIdAsync(id);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(tenant.Name, result.Name);
            Assert.AreEqual(tenant.Host, result.Host);
        }

        [TestMethod]
        public async Task FindByNameAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByNameAsync("Test Tenant", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByNameAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByNameAsync("Test Tenant"));
        }

        [TestMethod]
        public async Task FindByNameAsync_Finds_In_Blob_Storage()
        {
            const string blobName = "FindByName";
            var name = "Test Tenant";
            var tenant = new Tenant { Name = name, Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            var result = await _sut.FindByNameAsync(name);

            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(tenant.Host, result.Host);
        }

        [TestMethod]
        public async Task FindByHostAsync_Throws_If_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByHostAsync("testhost.jc", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByHostAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByHostAsync("testhost.jc"));
        }

        [TestMethod]
        public async Task FindByHostAsync_Finds_In_Blob_Storage()
        {
            const string blobName = "FindByHost";
            var host = "testhost.jc";
            var tenant = new Tenant { Name = "Test Tenant", Host = host };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            var result = await _sut.FindByHostAsync(host);

            Assert.IsNotNull(result);
            Assert.AreEqual(tenant.Name, result.Name);
            Assert.AreEqual(host, result.Host);
        }

        [TestMethod]
        public async Task SetName_Throws_Cancellation_When_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.SetNameAsync(tenant, "Test Tenant", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task SetName_Throws_When_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.SetNameAsync(tenant, "Test Tenant"));
        }

        [TestMethod]
        public async Task SetName_Sets_Tenant_Name_And_Does_Not_Save()
        {
            const string blobName = "SetName";
            var name = "Test Tenant";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            await _sut.SetNameAsync(tenant, name);

            var result = await LoadBlob(blobName);

            Assert.AreEqual(1, result.Count);
            Assert.AreNotEqual(name, result.First().Name);
        }

        [TestMethod]
        public async Task SetHost_Throws_Cancellation_When_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.SetHostAsync(tenant, "testhost.jc", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task SetHost_Throws_When_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_service, _options);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.SetHostAsync(tenant, "testhost.jc"));
        }

        [TestMethod]
        public async Task SetHost_Saves_In_Blob_Storage()
        {
            const string blobName = "SetHost";
            var host = "hostname.jc";
            var tenant = new Tenant { Name = "Existing Tenant", Host = "testhost.jc" };
            var tenants = new List<Tenant>() { tenant };
            await UploadBlob(blobName, tenants);

            await _sut.SetHostAsync(tenant, host);

            var result = await LoadBlob(blobName);

            Assert.AreEqual(1, result.Count);
            Assert.AreNotEqual(host, result.First().Host);
        }

        private async Task UploadBlob(string blobName, List<Tenant> tenants)
        {
            _options.BlobName = blobName;
            var blob = _container.GetBlobClient(_options.BlobName);

            var data = JsonSerializer.SerializeToUtf8Bytes(tenants);

            using (var stream = new MemoryStream(data))
                await blob.UploadAsync(stream);
        }

        private async Task<List<Tenant>> LoadBlob(string blobName)
        {
            _options.BlobName = blobName;
            var blob = _container.GetBlobClient(_options.BlobName);

            var download = await blob.DownloadAsync();
            using (var stream = new MemoryStream())
            {
                var tenants = await JsonSerializer.DeserializeAsync<List<Tenant>>(download.Value.Content);

                return tenants;
            }
        }
    }
}
