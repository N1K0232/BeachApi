using BeachApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApi.DataAccessLayer.Configurations.Common;

public abstract class TenantEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity> where TEntity : TenantEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(x => x.TenantId).IsRequired();
        base.Configure(builder);
    }
}