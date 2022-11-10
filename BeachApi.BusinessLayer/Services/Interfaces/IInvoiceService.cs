using BeachApi.Shared.Common;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;
public interface IInvoiceService
{
    Task<Result> DeleteAsync(Guid invoiceId);
    Task<ListResult<Invoice>> GetAsync();
    Task<Result<Invoice>> GetAsync(Guid invoiceId);
    Task<Result<Invoice>> SaveAsync(SaveInvoiceRequest request);
}