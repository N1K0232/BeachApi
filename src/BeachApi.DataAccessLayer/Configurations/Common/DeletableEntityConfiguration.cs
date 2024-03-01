using BeachApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations.Common;

public class DeletableEntityConfiguration<TEntity> : TenantEntityConfiguration<TEntity> where TEntity : DeletableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedDate).IsRequired(false);

        base.Configure(builder);
    }
}