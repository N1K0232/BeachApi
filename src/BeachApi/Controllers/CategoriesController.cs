using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace BeachApi.Controllers;

public class CategoriesController : ControllerBase
{
    private readonly ICategoryService categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        this.categoryService = categoryService;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await categoryService.DeleteAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("{id:guid}", Name = "GetCategory")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await categoryService.GetAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(string name = null)
    {
        var result = await categoryService.GetListAsync(name);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Insert([FromBody] SaveCategoryRequest category)
    {
        var result = await categoryService.InsertAsync(category);
        return HttpContext.CreateResponse(result, "GetCategory", new { id = result.Content?.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveCategoryRequest category)
    {
        var result = await categoryService.UpdateAsync(id, category);
        return HttpContext.CreateResponse(result);
    }
}