using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.Property(s => s.CompanyName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.ContactName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.City).HasMaxLength(50).IsRequired();

        base.Configure(builder);
    }
}