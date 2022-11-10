using BeachApi.Authentication.Common;
using BeachApi.Authorization.Filters;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BeachApi.Controllers;

public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        this.invoiceService = invoiceService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid invoiceId)
    {
        var result = await invoiceService.DeleteAsync(invoiceId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }


    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Get()
    {
        var invoices = await invoiceService.GetAsync();
        if (!invoices.Content.Any())
        {
            return NotFound("no invoice found");
        }

        return Ok(invoices);
    }

    [HttpGet("{invoiceId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Get(Guid invoiceId)
    {
        var invoiceResult = await invoiceService.GetAsync(invoiceId);
        return CreateResponse(invoiceResult, StatusCodes.Status200OK);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.Staff, RoleNames.PowerUser)]
    public async Task<IActionResult> Save([FromBody] SaveInvoiceRequest request)
    {
        var result = await invoiceService.SaveAsync(request);
        return CreateResponse(result, StatusCodes.Status201Created);
    }
}