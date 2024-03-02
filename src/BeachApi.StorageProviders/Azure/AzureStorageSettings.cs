namespace BeachApi.StorageProviders.Azure;

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = null!;

    public string? ContainerName { get; set; }
}