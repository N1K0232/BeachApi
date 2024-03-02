using System.Net.Mime;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace BeachApi.Controllers;

public class MeController : ControllerBase
{
    private readonly IAuthenticatedService authenticatedService;

    public MeController(IAuthenticatedService authenticatedService)
    {
        this.authenticatedService = authenticatedService;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProfilePhoto(IFormFile file)
    {
        var result = await authenticatedService.AddProfilePhotoAsync(file.OpenReadStream(), file.FileName);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("GetMe")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get()
    {
        var result = await authenticatedService.GetAsync();
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("GetProfilePhoto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfilePhoto()
    {
        var result = await authenticatedService.GetProfilePhotoAsync();
        return HttpContext.CreateResponse(result);
    }
}