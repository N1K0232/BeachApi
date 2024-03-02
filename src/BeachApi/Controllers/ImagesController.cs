using System.Net.Mime;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Extensions;
using BeachApi.Models;
using BeachApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace BeachApi.Controllers;

public class ImagesController : ControllerBase
{
    private readonly IImageService imageService;

    public ImagesController(IImageService imageService)
    {
        this.imageService = imageService;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await imageService.DeleteAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("{id:guid}", Name = "GetImage")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(Image), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await imageService.GetAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(IEnumerable<Image>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList()
    {
        var result = await imageService.GetListAsync();
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("{id:guid}/content")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Read(Guid id)
    {
        var result = await imageService.ReadAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [ProducesResponseType(typeof(Image), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] UploadImageRequest request)
    {
        var result = await imageService.UploadAsync(request.File.ToStreamFileContent(), request.Description);
        return HttpContext.CreateResponse(result, "GetImage", new { id = result.Content?.Id });
    }
}