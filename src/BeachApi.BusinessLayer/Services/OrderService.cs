using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Contracts;
using BeachApi.DataAccessLayer;
using BeachApi.Shared.Enums;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Common;
using BeachApi.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IDataContext dataContext;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public OrderService(IDataContext dataContext, IUserService userService, IMapper mapper)
    {
        this.dataContext = dataContext;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = dataContext.Get<Entities.Order>(trackingChanges: true).Include(o => o.OrderDetails).AsQueryable();
        var order = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (order is not null)
        {
            if (order.OrderDetails?.Count > 0)
            {
                dataContext.Delete(order.OrderDetails);
            }

            order.Status = OrderStatus.Canceled;
            dataContext.Delete(order);

            await dataContext.SaveAsync();
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<Result<Order>> GetAsync(Guid id)
    {
        var query = dataContext.Get<Entities.Order>();
        var dbOrder = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (dbOrder is not null)
        {
            var order = mapper.Map<Order>(dbOrder);
            order.OrderDetails = await LoadDetailsAsync(id);

            return order;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<Result<ListResult<Order>>> GetListAsync(DateTime? orderDate, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dataContext.Get<Entities.Order>();

        if (orderDate is not null)
        {
            query = query.Where(o => o.OrderDate == orderDate);
        }

        var totalCount = await query.CountAsync();

        var dbOrders = await query.OrderBy(orderBy)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ToListAsync();

        var orders = mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        return new ListResult<Order>(orders, totalCount, dbOrders.Count > itemsPerPage);
    }

    public async Task<Result<Order>> SaveAsync(SaveOrderRequest order)
    {
        try
        {
            var dbOrder = await SaveInternalAsync(order.OrderId);
            var dbProduct = await GetProductAsync(order.ProductId);

            if (dbProduct is null)
            {
                return Result.Fail(FailureReasons.ClientError, "No product found");
            }

            var orderDetail = new Entities.OrderDetail
            {
                OrderId = dbOrder.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Price = dbProduct.Price * order.Quantity,
                Annotations = order.Annotations
            };

            if (dbProduct.Quantity is not null)
            {
                dbProduct.Quantity -= order.Quantity;
            }

            dataContext.Insert(orderDetail);
            await dataContext.SaveAsync();

            var savedOrder = mapper.Map<Order>(dbOrder);
            return savedOrder;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(FailureReasons.ClientError, ex);
        }
    }

    private async Task<Entities.Product> GetProductAsync(Guid id)
    {
        var query = dataContext.Get<Entities.Product>(trackingChanges: true)
            .Where(p => p.Id == id);

        var exists = await query.AnyAsync();
        return exists ? await query.FirstAsync() : throw new ArgumentException("No product found", nameof(id));
    }

    private async Task<IEnumerable<OrderDetail>> LoadDetailsAsync(Guid orderId)
    {
        var query = dataContext.Get<Entities.OrderDetail>().Where(o => o.OrderId == orderId);
        var orderDetails = await query.ProjectTo<OrderDetail>(mapper.ConfigurationProvider)
            .ToListAsync();

        return orderDetails;
    }

    private async Task<Entities.Order> SaveInternalAsync(Guid? orderId)
    {
        if (orderId is null)
        {
            var dbOrder = new Entities.Order
            {
                Status = OrderStatus.New,
                UserId = userService.GetId()
            };

            dataContext.Insert(dbOrder);
            await dataContext.SaveAsync();

            return dbOrder;
        }
        else
        {
            var query = dataContext.Get<Entities.Order>().Where(o => o.Id == orderId);
            var exists = await query.AnyAsync();

            return exists ? await query.FirstAsync() : throw new ArgumentException("No order found", nameof(orderId));
        }
    }
}