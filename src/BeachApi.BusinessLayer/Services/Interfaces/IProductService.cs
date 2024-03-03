using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IProductService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Product>> GetAsync(Guid id);

    Task<Result<ListResult<Product>>> GetListAsync(string name, string orderBy, int pageIndex, int itemsPerPage);

    Task<Result<Product>> InsertAsync(SaveProductRequest product);

    Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest product);
}