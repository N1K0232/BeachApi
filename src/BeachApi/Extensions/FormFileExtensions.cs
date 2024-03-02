using OperationResults;

namespace BeachApi.Extensions;

public static class FormFileExtensions
{
    public static StreamFileContent ToStreamFileContent(this IFormFile file)
    {
        var content = new StreamFileContent(file.OpenReadStream(), null, file.FileName);
        return content;
    }
}