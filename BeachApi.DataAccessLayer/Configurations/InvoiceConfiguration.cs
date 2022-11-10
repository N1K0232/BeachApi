using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class InvoiceConfiguration : BaseEntityConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.Property(i => i.Title).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(4000).IsRequired();

        builder.Property(i => i.InvoiceDate).IsRequired();
        builder.Property(i => i.Price).HasPrecision(5, 2).IsRequired();

        base.Configure(builder);
    }
}