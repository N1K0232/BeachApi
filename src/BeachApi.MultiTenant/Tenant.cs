namespace BeachApi.MultiTenant;

public record class Tenant(Guid Id, string ConnectionString, string StorageConnectionString, string ContainerName);