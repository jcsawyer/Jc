using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Sequences;

using Jc.Core;
using Jc.MultiTenancy.Stores;

namespace Jc.MultiTenancy.EntityFramework.Tests
{
    [TestClass]
    public class TenantStoreTests
    {
        private readonly Mock<TenantDbContext> _context;
        private readonly Mock<DbSet<Tenant>> _tenants;

        private readonly TenantStore<Tenant> _sut;

        public TenantStoreTests()
        {
            _context = new Mock<TenantDbContext>();
            _tenants = new Mock<DbSet<Tenant>>();

            _sut = new TenantStore<Tenant>(_context.Object);
        }

        [TestMethod]
        public void Constructor_Throws_When_Context_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore(null));

            Assert.AreEqual("context", exception.ParamName);
        }

        [TestMethod]
        public void Generic_Tenant_Constructor_Throws_When_Context_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore<Tenant>(null));

            Assert.AreEqual("context", exception.ParamName);
        }

        [TestMethod]
        public void Generic_Tenant_And_Context_Throws_When_Context_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantStore<Tenant, TenantDbContext>(null));

            Assert.AreEqual("context", exception.ParamName);
        }

        [TestMethod]
        public void Tenants_Returns_Context_Tenant_Set()
        {
            _context.Setup(x => x.Set<Tenant>()).Returns(_tenants.Object).Verifiable();

            var tenants = _sut.Tenants;

            _context.Verify();
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
        public async Task CreateAsync_Throw_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.CreateAsync(tenant));
        }

        [TestMethod]
        public async Task CreateAsync_Adds_Tenant_To_Context()
        {
            var tenant = new Tenant();

            _context.Setup(x => x.Add(It.IsAny<Tenant>())).Verifiable();

            var result = await _sut.CreateAsync(tenant);

            _context.Verify();
            Assert.AreEqual(JcResult.Success, result);
        }

        [TestMethod]
        public async Task CreateAsync_Saves_Context_When_AutoSaveChanges_Is_True()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = true;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.CreateAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(JcResult.Success, result);
        }

        [TestMethod]
        public async Task CreateAsync_Does_Not_Save_Context_When_AutoSaveChanges_Is_False()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = false;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.CreateAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.AreEqual(JcResult.Success, result);
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
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.UpdateAsync(tenant));
        }

        [TestMethod]
        public async Task UpdateAsync_Attaches_Tenant_Entity_In_Context()
        {
            var tenant = new Tenant();

            _context.Setup(x => x.Attach(It.IsAny<Tenant>())).Verifiable();

            await _sut.UpdateAsync(tenant);

            _context.Verify(x => x.Attach(It.IsAny<Tenant>()), Times.Once);
            
        }

        [TestMethod]
        public async Task UpdateAsync_Updates_Tenant_In_Context()
        {
            var tenant = new Tenant();

            _context.Setup(x => x.Update(It.IsAny<Tenant>())).Verifiable();

            await _sut.UpdateAsync(tenant);

            _context.Verify(x => x.Update(It.IsAny<Tenant>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateAsync_Attaches_Before_Updating_In_Context()
        {
            var tenant = new Tenant();

            using (Sequence.Create())
            {
                _context.Setup(x => x.Attach(It.IsAny<Tenant>())).InSequence(Times.Once());
                _context.Setup(x => x.Update(It.IsAny<Tenant>())).InSequence(Times.Once());

                await _sut.UpdateAsync(tenant);
            }

            _context.Verify();
        }

        [TestMethod]
        public async Task UpdateAsync_Saves_Context_When_AutoSaveChanges_Is_True()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = true;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.UpdateAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(JcResult.Success, result);
        }

        [TestMethod]
        public async Task UpdateAsync_Does_Not_Save_Context_When_AutoSaveChanges_Is_False()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = false;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.UpdateAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.AreEqual(JcResult.Success, result);
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
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var tenant = new Tenant();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.DeleteAsync(tenant));
        }

        [TestMethod]
        public async Task DeleteAsync_Removes_Tenant_From_Context()
        {
            var tenant = new Tenant();

            _context.Setup(x => x.Remove(It.IsAny<Tenant>())).Verifiable();

            var result = await _sut.DeleteAsync(tenant);

            _context.Verify();
        }

        [TestMethod]
        public async Task DeleteAsync_Saves_Context_When_AutoSaveChanges_Is_True()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = true;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.DeleteAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(JcResult.Success, result);
        }

        [TestMethod]
        public async Task DeleteAsync_Does_Not_Save_Context_When_AutoSaveChanges_Is_False()
        {
            var tenant = new Tenant();

            _sut.AutoSaveChanges = false;

            _context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.DeleteAsync(tenant);

            _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.AreEqual(JcResult.Success, result);
        }

        [TestMethod]
        public async Task FindByIdAsync_Throws_If_Cancellation_Requested()
        {
            var id = Guid.NewGuid();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByIdAsync(id, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByIdAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var id = Guid.NewGuid();

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByIdAsync(id));
        }

        [TestMethod]
        public async Task FindByIdAsync_Finds_In_Context()
        {
            var id = Guid.NewGuid();

            _context.Setup(x => x.Set<Tenant>()).Returns(_tenants.Object);
            _tenants.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.FindByIdAsync(id);

            _tenants.Verify();
        }

        [TestMethod]
        public async Task FindByNameAsync_Throws_If_Cancellation_Requested()
        {
            var name = "Tenant Name";

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByNameAsync(name, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByNameAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var name = "Tenant Name";

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByNameAsync(name));
        }

        [TestMethod]
        public async Task FindByNameAsync_Finds_By_Name_In_Context()
        {
            var name = "Tenant Name";

            var tenants = new List<Tenant>()
            {
                new Tenant() { Id = Guid.NewGuid(), Name = "Tenant Name" }
            };

            var tenantsMock = Mocks.DbSetMock.CreateAsyncDbSetMock(tenants);
            _context.Setup(x => x.Set<Tenant>()).Returns(tenantsMock.Object);

            var result = await _sut.FindByNameAsync(name);

            Assert.IsNotNull(result);
            Assert.AreEqual(tenants[0], result);
        }

        [TestMethod]
        public async Task FindByHostAsync_Throws_If_Cancellation_Requested()
        {
            var host = "Tenant Name";

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await _sut.FindByHostAsync(host, cancellationTokenSource.Token));
        }

        [TestMethod]
        public async Task FindByHostAsync_Throws_If_Disposed()
        {
            var disposableStore = new TenantStore<Tenant>(_context.Object);
            var host = "Tenant Name";

            disposableStore.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => await disposableStore.FindByHostAsync(host));
        }

        [TestMethod]
        public async Task FindByHostAsync_Finds_By_Host_In_Context()
        {
            var host = "test.multitenancy.com";

            var tenants = new List<Tenant>()
            {
                new Tenant() { Id = Guid.NewGuid(), Host = host }
            };

            var tenantsMock = Mocks.DbSetMock.CreateAsyncDbSetMock(tenants);
            _context.Setup(x => x.Set<Tenant>()).Returns(tenantsMock.Object);

            var result = await _sut.FindByHostAsync(host);

            Assert.IsNotNull(result);
            Assert.AreEqual(tenants[0], result);
        }
    }
}
