using BeachApi.DataAccessLayer.Entities.Common;
using BeachApi.Shared.Enums;

namespace BeachApi.DataAccessLayer.Entities;

public class Order : DeletableEntity
{
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime OrderDate { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}