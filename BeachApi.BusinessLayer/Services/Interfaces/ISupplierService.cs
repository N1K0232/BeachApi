using BeachApi.Shared.Common;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface ISupplierService
{
    Task<Result> DeleteAsync(Guid supplierId);

    Task<ListResult<Supplier>> GetAsync();

    Task<Result<Supplier>> GetAsync(Guid supplierId);

    Task<Result<Supplier>> SaveAsync(SaveSupplierRequest request);
}