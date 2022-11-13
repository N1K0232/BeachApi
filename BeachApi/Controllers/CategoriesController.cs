using BeachApi.Authentication.Common;
using BeachApi.Authorization.Filters;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BeachApi.Controllers;

public class CategoriesController : ControllerBase
{
    private readonly ICategoryService categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        this.categoryService = categoryService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff)]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        var result = await categoryService.DeleteAsync(categoryId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff, RoleNames.User, RoleNames.Customer)]
    public async Task<IActionResult> Get()
    {
        var categories = await categoryService.GetAsync();
        if (categories.Content.Any())
        {
            return Ok(categories);
        }

        return NotFound("no category found");
    }

    [HttpGet("{categoryId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff, RoleNames.User, RoleNames.Customer)]
    public async Task<IActionResult> Get(Guid categoryId)
    {
        var category = await categoryService.GetAsync(categoryId);
        return CreateResponse(category, StatusCodes.Status200OK);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff)]
    public async Task<IActionResult> Save([FromBody] SaveCategoryRequest request)
    {
        var result = await categoryService.SaveAsync(request);
        return CreateResponse(result, StatusCodes.Status201Created);
    }
}