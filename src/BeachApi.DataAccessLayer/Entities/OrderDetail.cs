using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class OrderDetail : DeletableEntity
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string[] Annotations { get; set; }

    public virtual Order Order { get; set; }

    public virtual Product Product { get; set; }
}