using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class Invoice : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal Price { get; set; }
}