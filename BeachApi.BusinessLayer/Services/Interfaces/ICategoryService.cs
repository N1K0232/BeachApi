using BeachApi.Shared.Common;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface ICategoryService
{
    Task<Result> DeleteAsync(Guid categoryId);

    Task<ListResult<Category>> GetAsync();

    Task<Result<Category>> GetAsync(Guid categoryId);

    Task<Result<Category>> SaveAsync(SaveCategoryRequest request);
}