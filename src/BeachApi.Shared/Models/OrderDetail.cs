using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class OrderDetail : BaseObject
{
    public string Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string[]? Annotations { get; set; }
}