namespace BeachApi.DataAccessLayer.Entities.Common;

public abstract class DeletableEntity : TenantEntity
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}