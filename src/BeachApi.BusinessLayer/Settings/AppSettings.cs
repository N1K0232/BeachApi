namespace BeachApi.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationName { get; init; }

    public string StorageFolder { get; init; }

    public string[] SupportedCultures { get; init; }
}