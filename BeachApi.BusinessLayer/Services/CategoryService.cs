using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.DataAccessLayer;
using BeachApi.Shared.Common;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveCategoryRequest> categoryValidator;

    public CategoryService(IApplicationDataContext dataContext, IMapper mapper, IValidator<SaveCategoryRequest> categoryValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.categoryValidator = categoryValidator;
    }


    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetAsync<Entities.Category>(categoryId);
        if (category is not null)
        {
            dataContext.Delete(category);
            var deleteResult = await dataContext.SaveAsync();
            if (deleteResult)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "cannot delete category");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found");
    }

    public async Task<ListResult<Category>> GetAsync()
    {
        var categories = await dataContext.GetData<Entities.Category>()
            .OrderBy(c => c.Name)
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Category>(categories);
        return result;
    }

    public async Task<Result<Category>> GetAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetData<Entities.Category>()
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is not null)
        {
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No category found");
    }

    public async Task<Result<Category>> SaveAsync(SaveCategoryRequest request)
    {
        var validationResult = await categoryValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();

            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Validation errors", validationErrors);
        }

        var category = request.Id != null ? await dataContext.GetData<Entities.Category>(trackingChanges: true)
            .FirstOrDefaultAsync(c => c.Id == request.Id) : null;

        if (category is null)
        {
            category = mapper.Map<Entities.Category>(request);

            var categoryExists = await dataContext.ExistsAsync<Entities.Category>(c => c.Name == category.Name);
            if (categoryExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the category already exists");
            }

            dataContext.Insert(category);
        }
        else
        {
            mapper.Map(request, category);
            dataContext.Edit(category);
        }

        var savedResult = await dataContext.SaveAsync();
        if (savedResult)
        {
            var savedCategory = mapper.Map<Category>(category);
            return savedCategory;
        }

        return Result.Fail(FailureReasons.DatabaseError, "cannot save the category");
    }
}