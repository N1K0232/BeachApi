namespace BeachApi.Shared.Models.Requests;

public class SaveProductRequest
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public decimal Price { get; set; }
}