using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Jc.Core;

namespace Jc.MultiTenancy.Tests
{
    [TestClass]
    public class TenantManagerTests
    {
        private readonly TenantManager<ITenant> _sut;

        private readonly Mock<ITenantStore<ITenant>> _store;
        private readonly Mock<IOptions<MultiTenancyOptions>> _options;
        private readonly Mock<ILogger<TenantManager<ITenant>>> _logger;
        private readonly Mock<ITenant> _tenant;

        public TenantManagerTests()
        {
            _store = new Mock<ITenantStore<ITenant>>();
            _options = new Mock<IOptions<MultiTenancyOptions>>();
            _logger = new Mock<ILogger<TenantManager<ITenant>>>();
            _tenant = new Mock<ITenant>();

            _sut = new TenantManager<ITenant>(_store.Object, _options.Object, _logger.Object);
        }

        [TestMethod]
        public void Constructor_Sets_Options_And_Logger()
        {
            var manager = new TenantManager<ITenant>(_store.Object, _options.Object, _logger.Object);

            Assert.IsNotNull(manager);
            Assert.IsNotNull(manager.Options);
            Assert.IsNotNull(manager.Logger);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_Store_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantManager<ITenant>(null, _options.Object, _logger.Object));

            Assert.AreEqual("store", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Throws_ArgumentNull_When_Logger_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new TenantManager<ITenant>(_store.Object, _options.Object, null));

            Assert.AreEqual("logger", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_Sets_Default_Options_When_Options_Is_Null()
        {
            var manager = new TenantManager<ITenant>(_store.Object, null, _logger.Object);
            
            Assert.IsNotNull(manager.Options);
        }

        [TestMethod]
        public void Tenants_Throws_NotSupported_When_Does_Not_Implement_IQueryableTenantStore()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => _sut.Tenants);
            Assert.AreEqual("Store does not implement IQueryableTenantStore<TTenant>", exception.Message);
        }

        [TestMethod]
        public void Tenants_Returns_Queryable_When_Store_Implements_IQueryableTenantStore()
        {
            var queryableStore = new Mock<IQueryableTenantStore<ITenant>>();
            var manager = new TenantManager<ITenant>(queryableStore.Object, _options.Object, _logger.Object);

            var tenants = manager.Tenants;
            Assert.IsNotNull(tenants);
            Assert.IsInstanceOfType(tenants, typeof(IQueryable<ITenant>));
        }

        [TestMethod]
        public async Task CreateAsync_Calls_Store_CreateAsync()
        {
            _store.Setup(x => x.CreateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.CreateAsync(_tenant.Object);

            _store.Verify();
        }

        [TestMethod]
        public async Task CreateAsync_Returns_Success_Result_When_Store_Successful()
        {
            _store.Setup(x => x.CreateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Success));

            var result = await _sut.CreateAsync(_tenant.Object);

            Assert.AreEqual(true, result.Succeeded);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [TestMethod]
        public async Task CreateAsync_Returns_Error_Result_With_Errprs_When_Store_Unsucessful()
        {
            var error = new JcError { Code = "T35T", Description = "Testing" };
            _store.Setup(x => x.CreateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Failed(error)));

            var result = await _sut.CreateAsync(_tenant.Object);

            Assert.AreEqual(false, result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual(error, result.Errors.First());
        }

        [TestMethod]
        public async Task UpdateAsync_Calls_Store_UpdateAsync()
        {
            _store.Setup(x => x.UpdateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.UpdateAsync(_tenant.Object);

            _store.Verify();
        }

        [TestMethod]
        public async Task UpdateAsync_Returns_Success_Result_When_Store_Successful()
        {
            _store.Setup(x => x.UpdateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Success));

            var result = await _sut.UpdateAsync(_tenant.Object);

            Assert.AreEqual(true, result.Succeeded);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_Returns_Error_Result_With_Errprs_When_Store_Unsucessful()
        {
            var error = new JcError { Code = "T35T", Description = "Testing" };
            _store.Setup(x => x.UpdateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Failed(error)));

            var result = await _sut.UpdateAsync(_tenant.Object);

            Assert.AreEqual(false, result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual(error, result.Errors.First());
        }

        [TestMethod]
        public async Task DeleteAsync_Calls_Store_DeleteAsync()
        {
            _store.Setup(x => x.DeleteAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.DeleteAsync(_tenant.Object);

            _store.Verify();
        }

        [TestMethod]
        public async Task DeleteAsync_Returns_Success_Result_When_Store_Successful()
        {
            _store.Setup(x => x.DeleteAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Success));

            var result = await _sut.DeleteAsync(_tenant.Object);

            Assert.AreEqual(true, result.Succeeded);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [TestMethod]
        public async Task DeleteAsync_Returns_Error_Result_With_Errprs_When_Store_Unsucessful()
        {
            var error = new JcError { Code = "T35T", Description = "Testing" };
            _store.Setup(x => x.DeleteAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(JcResult.Failed(error)));

            var result = await _sut.DeleteAsync(_tenant.Object);

            Assert.AreEqual(false, result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual(error, result.Errors.First());
        }

        [TestMethod]
        public async Task FindByIdAsync_Calls_Store_FindByIdAsync()
        {
            _store.Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.FindByIdAsync(Guid.NewGuid());

            _store.Verify();
        }

        [TestMethod]
        public async Task FindByIdAsync_Returns_Tenant_When_Store_Finds_Match()
        {
            _store.Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(_tenant.Object));

            var result = await _sut.FindByIdAsync(Guid.NewGuid());

            Assert.IsNotNull(result);
            Assert.AreEqual(_tenant.Object, result);
        }

        [TestMethod]
        public async Task FindByIdAsync_Returns_Null_When_Store_Does_Not_Find_Match()
        {
            _store.Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult<ITenant>(null));

            var result = await _sut.FindByIdAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByNameAsync_Calls_Store_FindByNameAsync()
        {
            _store.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.FindByNameAsync("Name");

            _store.Verify();
        }

        [TestMethod]
        public async Task FindByNameAsync_Returns_Tenant_When_Store_Finds_Match()
        {
            _store.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(_tenant.Object));

            var result = await _sut.FindByNameAsync("Name");

            Assert.IsNotNull(result);
            Assert.AreEqual(_tenant.Object, result);
        }

        [TestMethod]
        public async Task FindByNameAsync_Returns_Null_When_Store_Does_Not_Find_Match()
        {
            _store.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult<ITenant>(null));

            var result = await _sut.FindByNameAsync("Name");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByHostAsync_Calls_Store_FindByHostAsync()
        {
            _store.Setup(x => x.FindByHostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Verifiable();

            await _sut.FindByHostAsync("Host");

            _store.Verify();
        }

        [TestMethod]
        public async Task FindByHostAsync_Returns_Tenant_When_Store_Finds_Match()
        {
            _store.Setup(x => x.FindByHostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult(_tenant.Object));

            var result = await _sut.FindByHostAsync("Host");

            Assert.IsNotNull(result);
            Assert.AreEqual(_tenant.Object, result);
        }

        [TestMethod]
        public async Task FindByHostAsync_Returns_Null_When_Store_Does_Not_Find_Match()
        {
            _store.Setup(x => x.FindByHostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () => await Task.FromResult<ITenant>(null));

            var result = await _sut.FindByHostAsync("Host");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetNameAsync_Calls_Store_SetNameAsync_And_UpdateAsync()
        {
            _store.Setup(x => x.SetNameAsync(It.IsAny<ITenant>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Verifiable();
            _store.Setup(x => x.UpdateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.SetNameAsync(_tenant.Object, "Name");

            _store.Verify();
        }

        [TestMethod]
        public async Task SetHostAsync_Calls_Store_SetHostAsync_And_UpdateAsync()
        {
            _store.Setup(x => x.SetHostAsync(It.IsAny<ITenant>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Verifiable();
            _store.Setup(x => x.UpdateAsync(It.IsAny<ITenant>(), It.IsAny<CancellationToken>())).Verifiable();

            var result = await _sut.SetHostAsync(_tenant.Object, "Host");

            _store.Verify();
        }
    }
}
