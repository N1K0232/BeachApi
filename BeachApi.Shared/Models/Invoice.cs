using BeachApi.Shared.Common;

namespace BeachApi.Shared.Models;

public class Invoice : BaseModel
{
    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal Price { get; set; }
}