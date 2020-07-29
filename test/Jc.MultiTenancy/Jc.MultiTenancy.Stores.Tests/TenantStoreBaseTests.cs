using Jc.MultiTenancy.Stores.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Stores.Tests
{
    [TestClass]
    public class TenantStoreBaseTests
    {
        private readonly MockTenantStoreBase _sut;

        public TenantStoreBaseTests()
        {
            _sut = new MockTenantStoreBase();
        }

        [TestMethod]
        public async Task SetNameAsync_Sets_Name()
        {
            var tenant = new Tenant();
            string expected = "Tenant Name";

            await _sut.SetNameAsync(tenant, expected);

            Assert.AreEqual(expected, tenant.Name);
        }

        [TestMethod]
        public async Task SetNameAsync_Throws_When_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.SetNameAsync(tenant, "Tenant Name", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task SetNameAsync_Throws_When_Disposed()
        {
            var disposableMock = new MockTenantStoreBase();
            var tenant = new Tenant();

            disposableMock.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableMock.SetNameAsync(tenant, "Tenant Name"));
        }

        [TestMethod]
        public async Task SetHostAsync_Sets_Host()
        {
            var tenant = new Tenant();
            string expected = "tenant.host";

            await _sut.SetHostAsync(tenant, expected);

            Assert.AreEqual(expected, tenant.Host);
        }

        [TestMethod]
        public async Task SetHostAsync_Throws_When_Cancellation_Requested()
        {
            var tenant = new Tenant();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.SetHostAsync(tenant, "tenant.host", cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task SetHostAsync_Throws_When_Disposed()
        {
            var disposableMock = new MockTenantStoreBase();
            var tenant = new Tenant();

            disposableMock.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableMock.SetHostAsync(tenant, "tenant.host"));
        }
    }
}
