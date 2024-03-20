using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TinyHelpers.EntityFrameworkCore.Extensions;

namespace BeachApi.DataAccessLayer.Configurations;

public class OrderDetailConfiguration : DeletableEntityConfiguration<OrderDetail>
{
    public override void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.Property(o => o.Quantity).IsRequired();
        builder.Property(o => o.Price).HasPrecision(8, 2).IsRequired();
        builder.Property(o => o.Annotations).HasArrayConversion().HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.HasOne(o => o.Order).WithMany(o => o.OrderDetails).HasForeignKey(o => o.OrderId).IsRequired();
        builder.HasOne(o => o.Product).WithMany(o => o.OrderDetails).HasForeignKey(o => o.ProductId).IsRequired();

        builder.ToTable("OrderDetails");
        base.Configure(builder);
    }
}