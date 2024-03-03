using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace BeachApi.Controllers;

public class ProductsController : ControllerBase
{
    private readonly IProductService productService;

    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await productService.DeleteAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("{id:guid}", Name = "GetProduct")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await productService.GetAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(ListResult<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(string name = null, string orderBy = "Name, Price", int pageIndex = 0, int itemsPerPage = 10)
    {
        var result = await productService.GetListAsync(name, orderBy, pageIndex, itemsPerPage);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Insert([FromBody] SaveProductRequest product)
    {
        var result = await productService.InsertAsync(product);
        return HttpContext.CreateResponse(result, "GetProduct", new { id = result.Content?.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveProductRequest product)
    {
        var result = await productService.UpdateAsync(id, product);
        return HttpContext.CreateResponse(result);
    }
}