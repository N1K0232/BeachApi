using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class Product : DeletableEntity
{
    public string Name { get; set; }

    public string Description { get; set; }

    public int? Quantity { get; set; }

    public decimal Price { get; set; }

    public Guid CategoryId { get; set; }

    public virtual Category Category { get; set; }
}