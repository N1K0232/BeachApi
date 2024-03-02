using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class CategoryConfiguration : TenantEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(512).IsRequired(false);

        builder.ToTable("Categories");
        base.Configure(builder);
    }
}