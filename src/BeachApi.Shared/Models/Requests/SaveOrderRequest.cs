namespace BeachApi.Shared.Models.Requests;

public class SaveOrderRequest
{
    public Guid? OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public string[]? Annotations { get; set; }
}