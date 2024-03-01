using BeachApi.Authentication.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeachApi.Authentication;

public class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
            builder.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);
        });

        modelBuilder.Entity<ApplicationUserRole>(builder =>
        {
            builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            builder.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .IsRequired();

            builder.HasOne(userRole => userRole.Role)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<DataProtectionKey>(builder =>
        {
            builder.ToTable("DataProtectionKeys");
            builder.HasKey(key => key.Id);

            builder.Property(key => key.FriendlyName).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
            builder.Property(key => key.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });

        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.ToTable("Tenants");
            builder.HasKey(tenant => tenant.Id);
            builder.Property(tenant => tenant.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

            builder.Property(tenant => tenant.ConnectionString).HasMaxLength(4000).IsRequired().IsUnicode(false);
            builder.Property(tenant => tenant.StorageConnectionString).HasMaxLength(4000).IsRequired(false).IsUnicode(false);
            builder.Property(tenant => tenant.ContainerName).HasMaxLength(256).IsRequired(false).IsUnicode(false);
        });
    }
}