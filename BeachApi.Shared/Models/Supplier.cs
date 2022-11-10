using BeachApi.Shared.Common;

namespace BeachApi.Shared.Models;

public class Supplier : BaseModel
{
    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string City { get; set; }
}