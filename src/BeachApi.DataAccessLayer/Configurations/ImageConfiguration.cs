using BeachApi.DataAccessLayer.Configurations.Common;
using BeachApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations;

public class ImageConfiguration : TenantEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.Property(i => i.Path).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(512).IsRequired(false);

        builder.HasIndex(i => i.Path).HasDatabaseName("IX_Path").IsUnique();

        builder.ToTable("Images");
        base.Configure(builder);
    }
}