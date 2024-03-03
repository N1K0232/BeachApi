using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class Product : BaseObject
{
    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public decimal Price { get; set; }
}