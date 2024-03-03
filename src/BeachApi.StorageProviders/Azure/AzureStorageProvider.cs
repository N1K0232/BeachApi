using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace BeachApi.StorageProviders.Azure;

internal class AzureStorageProvider : IStorageProvider
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly AzureStorageSettings settings;

    public AzureStorageProvider(AzureStorageSettings settings)
    {
        blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        this.settings = settings;
    }

    public async Task DeleteAsync(string path)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.DeleteBlobIfExistsAsync(blobName);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path);
        var blobExists = await blobClient.ExistsAsync();

        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync();
        return stream;
    }

    public async Task UploadAsync(Stream stream, string path, bool overwrite = false)
    {
        var blobClient = await GetBlobClientAsync(path, true);
        if (!overwrite)
        {
            var blobExists = await blobClient.ExistsAsync();
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists.");
            }
        }

        var headers = new BlobHttpHeaders
        {
            ContentType = MimeUtility.GetMimeMapping(path)
        };

        stream.Position = 0;
        await blobClient.UploadAsync(stream, headers);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }

        return blobContainerClient.GetBlobClient(blobName);
    }

    private (string ContainerName, string BlobClient) ExtractContainerBlobName(string? path)
    {
        var fullPath = path?.Replace(@"\", "/") ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            return (settings.ContainerName.ToLowerInvariant(), fullPath);
        }

        var root = Path.GetPathRoot(fullPath);
        var fileName = fullPath[(root ?? string.Empty).Length..];
        var parts = fileName.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        return (containerName, blobName);
    }
}