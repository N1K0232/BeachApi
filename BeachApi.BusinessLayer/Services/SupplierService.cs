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

public class SupplierService : ISupplierService
{
    private readonly IApplicationDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveSupplierRequest> supplierValidator;

    public SupplierService(IApplicationDataContext dataContext, IMapper mapper, IValidator<SaveSupplierRequest> supplierValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.supplierValidator = supplierValidator;
    }


    public async Task<Result> DeleteAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var supplier = await dataContext.GetAsync<Entities.Supplier>(supplierId);
        if (supplier is not null)
        {
            dataContext.Delete(supplier);
            var result = await dataContext.SaveAsync();
            if (result)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "Cannot delete supplier");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No supplier found");
    }

    public async Task<ListResult<Supplier>> GetAsync()
    {
        var suppliers = await dataContext.GetData<Entities.Supplier>()
            .OrderBy(s => s.ContactName)
            .ProjectTo<Supplier>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Supplier>(suppliers);
        return result;
    }

    public async Task<Result<Supplier>> GetAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var supplier = await dataContext.GetData<Entities.Supplier>()
            .ProjectTo<Supplier>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(s => s.Id == supplierId);

        if (supplier is not null)
        {
            return supplier;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No supplier found");
    }

    public async Task<Result<Supplier>> SaveAsync(SaveSupplierRequest request)
    {
        var validationResult = await supplierValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var supplier = request.Id != null ? await dataContext.GetData<Entities.Supplier>(trackingChanges: true)
            .FirstOrDefaultAsync(s => s.Id == request.Id) : null;

        if (supplier is null)
        {
            supplier = mapper.Map<Entities.Supplier>(request);
            dataContext.Insert(supplier);
        }
        else
        {
            mapper.Map(request, supplier);
            dataContext.Edit(supplier);
        }

        var result = await dataContext.SaveAsync();
        if (result)
        {
            var savedSupplier = mapper.Map<Supplier>(supplier);
            return savedSupplier;
        }

        return Result.Fail(FailureReasons.DatabaseError, "Cannot save the supplier");
    }
}