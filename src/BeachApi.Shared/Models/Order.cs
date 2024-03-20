using BeachApi.Shared.Enums;
using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class Order : BaseObject
{
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public DateTime OrderDate { get; set; }

    public IEnumerable<OrderDetail>? OrderDetails { get; set; }
}