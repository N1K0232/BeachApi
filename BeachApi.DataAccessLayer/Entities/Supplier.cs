using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class Supplier : BaseEntity
{
    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string City { get; set; }
}