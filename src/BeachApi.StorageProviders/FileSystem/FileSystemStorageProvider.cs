namespace BeachApi.StorageProviders.FileSystem;

internal class FileSystemStorageProvider : IStorageProvider
{
    private readonly FileSystemStorageSettings settings;

    public FileSystemStorageProvider(FileSystemStorageSettings settings)
    {
        this.settings = settings;
    }

    public Task DeleteAsync(string path)
    {
        var fullPath = CreatePath(path);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> ReadAsync(string path)
    {
        var fullPath = CreatePath(path);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public async Task UploadAsync(Stream stream, string path, bool overwrite = false)
    {
        var fullPath = CreatePath(path);
        if (!overwrite)
        {
            if (File.Exists(fullPath))
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        await CreateDirectoryAsync(path);
        using var outputStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);

        stream.Position = 0;
        await stream.CopyToAsync(outputStream);
    }

    private Task CreateDirectoryAsync(string path)
    {
        var fullPath = CreatePath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return Task.CompletedTask;
    }

    private string CreatePath(string path)
    {
        var fullPath = Path.Combine(settings.StorageFolder, path);
        if (!Path.IsPathRooted(fullPath))
        {
            return Path.Combine(settings.SiteRootFolder, fullPath);
        }

        return fullPath;
    }
}