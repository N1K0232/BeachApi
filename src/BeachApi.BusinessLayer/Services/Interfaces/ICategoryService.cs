using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Requests;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface ICategoryService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Category>> GetAsync(Guid id);

    Task<Result<IEnumerable<Category>>> GetListAsync(string name);

    Task<Result<Category>> InsertAsync(SaveCategoryRequest category);

    Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest category);
}