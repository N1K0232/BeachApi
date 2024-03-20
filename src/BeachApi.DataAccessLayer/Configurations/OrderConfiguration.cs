using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class OrderConfiguration : DeletableEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(o => o.OrderDate).ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");

        builder.ToTable("Orders");
        base.Configure(builder);
    }
}