using BeachApi.Authentication.Common;
using BeachApi.Authorization.Filters;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BeachApi.Controllers;

public class SuppliersController : ControllerBase
{
    private readonly ISupplierService supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        this.supplierService = supplierService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid supplierId)
    {
        var result = await supplierService.DeleteAsync(supplierId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Get()
    {
        var suppliers = await supplierService.GetAsync();
        if (suppliers.Content.Any())
        {
            return Ok(suppliers);
        }

        return NotFound("No supplier found");
    }

    [HttpGet("{supplierId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Get(Guid supplierId)
    {
        var supplier = await supplierService.GetAsync(supplierId);
        return CreateResponse(supplier, StatusCodes.Status200OK);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Save([FromBody] SaveSupplierRequest request)
    {
        var result = await supplierService.SaveAsync(request);
        return CreateResponse(result, StatusCodes.Status201Created);
    }
}