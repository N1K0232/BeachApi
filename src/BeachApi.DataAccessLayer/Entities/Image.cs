using BeachApi.DataAccessLayer.Entities.Common;

namespace BeachApi.DataAccessLayer.Entities;

public class Image : TenantEntity
{
    public string Path { get; set; }

    public long Length { get; set; }

    public string Description { get; set; }
}