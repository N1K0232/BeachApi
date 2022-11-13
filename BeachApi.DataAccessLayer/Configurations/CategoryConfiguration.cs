using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(512).IsRequired(false);

        base.Configure(builder);
    }
}