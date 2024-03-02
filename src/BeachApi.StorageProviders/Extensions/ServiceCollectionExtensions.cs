using BeachApi.StorageProviders.Azure;
using BeachApi.StorageProviders.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApi.StorageProviders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<AzureStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var azureStorageSettings = new AzureStorageSettings();
        configuration.Invoke(azureStorageSettings);

        services.AddSingleton(azureStorageSettings);
        services.AddScoped<IStorageProvider, AzureStorageProvider>();

        return services;
    }

    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<IServiceProvider, AzureStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services.AddScoped(provider =>
        {
            var azureStorageSettings = new AzureStorageSettings();
            configuration.Invoke(provider, azureStorageSettings);

            return azureStorageSettings;
        });

        services.AddScoped<IStorageProvider, AzureStorageProvider>();
        return services;
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var fileSystemStorageSettings = new FileSystemStorageSettings();
        configuration.Invoke(fileSystemStorageSettings);

        services.AddSingleton(fileSystemStorageSettings);
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;
    }
}