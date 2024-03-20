using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Order>> GetAsync(Guid id);

    Task<Result<ListResult<Order>>> GetListAsync(DateTime? orderDate, int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<Order>> SaveAsync(SaveOrderRequest order);
}