using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.DataAccessLayer;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;

    public CategoryService(IDataContext dataContext, IMapper mapper)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await dataContext.GetAsync<Entities.Category>(id);
        if (category is not null)
        {
            dataContext.Delete(category);
            await dataContext.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found");
    }

    public async Task<Result<Category>> GetAsync(Guid id)
    {
        var dbCategory = await dataContext.GetAsync<Entities.Category>(id);
        if (dbCategory is not null)
        {
            var category = mapper.Map<Category>(dbCategory);
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found");
    }

    public async Task<Result<IEnumerable<Category>>> GetListAsync(string name)
    {
        var query = dataContext.Get<Entities.Category>();

        if (name.HasValue())
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        var categories = await query.OrderBy(c => c.Name)
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .ToListAsync();

        return categories;
    }

    public async Task<Result<Category>> InsertAsync(SaveCategoryRequest category)
    {
        var query = dataContext.Get<Entities.Category>();
        var exists = await query.AnyAsync(c => c.Name == category.Name && c.Description == category.Description);

        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, "The same category already exists");
        }

        var dbCategory = mapper.Map<Entities.Category>(category);
        dataContext.Insert(dbCategory);
        await dataContext.SaveAsync();

        var savedCategory = mapper.Map<Category>(dbCategory);
        return savedCategory;
    }

    public async Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest category)
    {
        var query = dataContext.Get<Entities.Category>(trackingChanges: true);
        var dbCategory = await query.FirstOrDefaultAsync(c => c.Id == id);

        if (dbCategory is not null)
        {
            var exists = await query.AnyAsync(c => c.Name == category.Name && c.Description == category.Description);
            if (exists)
            {
                return Result.Fail(FailureReasons.Conflict, "This category already exists");
            }

            mapper.Map(category, dbCategory);
            await dataContext.SaveAsync();

            var savedCategory = mapper.Map<Category>(dbCategory);
            return savedCategory;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found");
    }
}