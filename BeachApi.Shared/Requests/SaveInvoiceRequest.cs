using BeachApi.Shared.Common;

namespace BeachApi.Shared.Requests;

public class SaveInvoiceRequest : BaseRequestModel
{
    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal Price { get; set; }
}