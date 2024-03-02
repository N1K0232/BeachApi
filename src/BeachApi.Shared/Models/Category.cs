using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class Category : BaseObject
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}