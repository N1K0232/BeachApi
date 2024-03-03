using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.DataAccessLayer;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;

    public ProductService(IDataContext dataContext, IMapper mapper)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var product = await dataContext.GetAsync<Entities.Product>(id);
        if (product is not null)
        {
            dataContext.Delete(product);
            await dataContext.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No product found");
    }

    public async Task<Result<Product>> GetAsync(Guid id)
    {
        var dbProduct = await dataContext.GetAsync<Entities.Product>(id);
        if (dbProduct is not null)
        {
            var product = mapper.Map<Product>(dbProduct);
            return product;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No product found");
    }

    public async Task<Result<ListResult<Product>>> GetListAsync(string name, string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = dataContext.Get<Entities.Product>().Include(p => p.Category).AsQueryable();

        if (name.HasValue())
        {
            query = query.Where(p => p.Name.Contains(name) || p.Category.Name.Contains(name));
        }

        var totalCount = await query.CountAsync();
        var dbProducts = await query.OrderBy(orderBy)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ToListAsync();

        var products = mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);
        return new ListResult<Product>(products, totalCount, dbProducts.Count > itemsPerPage);
    }

    public async Task<Result<Product>> InsertAsync(SaveProductRequest product)
    {
        var products = dataContext.Get<Entities.Product>();
        var categories = dataContext.Get<Entities.Category>();

        var categoryExists = await categories.AnyAsync(c => c.Id == product.CategoryId);
        if (!categoryExists)
        {
            return Result.Fail(FailureReasons.ClientError, "This category doesn't exists");
        }

        var productExists = await products.AnyAsync(p => p.Name == product.Name && p.Price == product.Price);
        if (productExists)
        {
            return Result.Fail(FailureReasons.Conflict, "This product already exists");
        }

        var dbProduct = mapper.Map<Entities.Product>(product);
        dataContext.Insert(dbProduct);

        await dataContext.SaveAsync();
        return mapper.Map<Product>(dbProduct);
    }

    public async Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest product)
    {
        var products = dataContext.Get<Entities.Product>(trackingChanges: true);
        var categories = dataContext.Get<Entities.Category>();

        var dbProduct = await products.FirstOrDefaultAsync(p => p.Id == id);
        if (dbProduct is not null)
        {
            var categoryExists = await categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                return Result.Fail(FailureReasons.ClientError, "This category doesn't exists");
            }

            var productExists = await products.AnyAsync(p => p.Name == product.Name && p.Price == product.Price);
            if (productExists)
            {
                return Result.Fail(FailureReasons.Conflict, "This product already exists");
            }

            mapper.Map(product, dbProduct);
            await dataContext.SaveAsync();

            return mapper.Map<Product>(dbProduct);
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No product found");
    }
}