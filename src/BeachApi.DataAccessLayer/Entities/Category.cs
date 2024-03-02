using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class Category : TenantEntity
{
    public string Name { get; set; }

    public string Description { get; set; }
}