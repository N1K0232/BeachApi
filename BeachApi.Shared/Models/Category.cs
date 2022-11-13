using BeachApi.Shared.Common;

namespace BeachApi.Shared.Models;

public class Category : BaseModel
{
    public string Name { get; set; }

    public string Description { get; set; }
}