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

    [HttpGet]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        var result = await authenticatedService.GetAsync();
        return HttpContext.CreateResponse(result);
    }
}