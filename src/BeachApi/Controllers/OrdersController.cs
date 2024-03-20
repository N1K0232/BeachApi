using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace BeachApi.Controllers;

public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await orderService.DeleteAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await orderService.GetAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpGet]
    [Authorize(Policy = "Administrator")]
    [ProducesResponseType(typeof(ListResult<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(DateTime? orderDate = null, int pageIndex = 0, int itemsPerPage = 10, string orderBy = "OrderDate DESC")
    {
        var result = await orderService.GetListAsync(orderDate, pageIndex, itemsPerPage, orderBy);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "UserActive")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save([FromBody] SaveOrderRequest order)
    {
        var result = await orderService.SaveAsync(order);
        return HttpContext.CreateResponse(result);
    }
}