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

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveInvoiceRequest> invoiceValidator;

    public InvoiceService(IApplicationDataContext dataContext, IMapper mapper, IValidator<SaveInvoiceRequest> invoiceValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.invoiceValidator = invoiceValidator;
    }

    public async Task<Result> DeleteAsync(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var invoice = await dataContext.GetAsync<Entities.Invoice>(invoiceId);
        if (invoice is not null)
        {
            dataContext.Delete(invoice);

            var result = await dataContext.SaveAsync();
            if (result)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "Errors while deleting");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "Invoice not found");
    }

    public async Task<ListResult<Invoice>> GetAsync()
    {
        var invoices = await dataContext.GetData<Entities.Invoice>()
            .OrderByDescending(i => i.InvoiceDate)
            .ProjectTo<Invoice>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Invoice>(invoices);
        return result;
    }

    public async Task<Result<Invoice>> GetAsync(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var invoice = await dataContext.GetData<Entities.Invoice>()
            .ProjectTo<Invoice>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is not null)
        {
            return invoice;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No invoice found");
    }

    public async Task<Result<Invoice>> SaveAsync(SaveInvoiceRequest request)
    {
        var validationResult = await invoiceValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();

            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid model", validationErrors);
        }

        var invoice = request.Id != null ? await dataContext.GetData<Entities.Invoice>(trackingChanges: true)
            .FirstOrDefaultAsync(i => i.Id == request.Id) : null;

        if (invoice is null)
        {
            invoice = mapper.Map<Entities.Invoice>(request);
            dataContext.Insert(invoice);
        }
        else
        {
            mapper.Map(request, invoice);
            dataContext.Edit(invoice);
        }

        var result = await dataContext.SaveAsync();
        if (result)
        {
            var savedInvoice = mapper.Map<Invoice>(invoice);
            return savedInvoice;
        }

        return Result.Fail(FailureReasons.DatabaseError, "Cannot save invoice");
    }
}