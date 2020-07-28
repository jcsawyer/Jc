using Microsoft.EntityFrameworkCore;

using Jc.MultiTenancy.Stores;

namespace Jc.MultiTenancy.EntityFramework
{
    /// <summary>
    /// Represents a database context for <see cref="Tenant"/>s
    /// </summary>
    public class TenantDbContext : TenantDbContext<Tenant>
    {
        /// <summary>
        /// Initializes a new <see cref="TenantDbContext"/>
        /// </summary>
        protected TenantDbContext() { }
    }

    /// <summary>
    /// Represents a database context for <typeparamref name="TTenant"/>s
    /// </summary>
    /// <typeparam name="TTenant">Type of tenant</typeparam>
    public class TenantDbContext<TTenant> : DbContext
        where TTenant : Tenant
    {
        /// <summary>
        /// Initializes a new <see cref="TenantDbContext{TTenant}"/>
        /// </summary>
        protected TenantDbContext() { }

        /// <summary>
        /// Initializes a new <see cref="TenantDbContext{TTenant}"/> with
        /// the specified <paramref name="options"/>
        /// </summary>
        /// <param name="options"><see cref="DbContextOptions"/> options</param>
        public TenantDbContext(DbContextOptions options) : base(options) { }

        /// <inheritdoc cref="DbSet{TTenant}"/>
        public DbSet<TTenant> Tenants { get; set; }

        /// <inheritdoc cref="DbContext.OnModelCreating(ModelBuilder)"/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TTenant>(x =>
            {
                x.ToTable("Tenants");
                x.HasKey(x => x.Id);
                x.HasIndex(x => x.Name)
                    .HasName("TenantNameIndex")
                    .IsUnique();

                x.Property(x => x.Name).IsRequired().HasMaxLength(265);
                x.Property(x => x.Host).IsRequired();
            });
        }
    }
}
