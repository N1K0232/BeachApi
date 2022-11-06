using System.Text;

namespace BeachApi.Security;

public static class StringHasher
{
    public static string GetString(string encodedString)
    {
        var bytes = Convert.FromBase64String(encodedString);
        var originalString = Encoding.UTF8.GetString(bytes);
        return originalString;
    }
}