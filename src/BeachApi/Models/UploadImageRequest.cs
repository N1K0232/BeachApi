using Microsoft.AspNetCore.Mvc.ModelBinding;
using TinyHelpers.AspNetCore.DataAnnotations;

namespace BeachApi.Models;

public class UploadImageRequest
{
    [BindRequired]
    [AllowedExtensions("*.jpg", "*.jpeg", "*.png")]
    public IFormFile File { get; set; }

    public string Description { get; set; }
}