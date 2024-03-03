using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class ProductConfiguration : DeletableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(512).IsRequired(false);

        builder.Property(p => p.Quantity).IsRequired(false);
        builder.Property(p => p.Price).HasPrecision(8, 2).IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .IsRequired();

        builder.ToTable("Products");
        base.Configure(builder);
    }
}