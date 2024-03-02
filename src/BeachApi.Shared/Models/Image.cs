using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class Image : BaseObject
{
    public string Path { get; set; } = null!;

    public long Length { get; set; }

    public string? Description { get; set; }
}