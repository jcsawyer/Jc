using Jc.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jc.MultiTenancy.Stores.Tests.Mocks
{
    public class MockTenantStoreBase : TenantStoreBase<Tenant>
    {
        public override IQueryable<Tenant> Tenants => throw new NotImplementedException();

        public override Task<JcResult> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<JcResult> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<JcResult> DeleteAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Tenant> FindByHostAsync(string host, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Tenant> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Tenant> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        
    }
}
