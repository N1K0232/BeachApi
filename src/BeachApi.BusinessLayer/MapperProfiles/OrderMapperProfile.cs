using AutoMapper;
using BeachApi.Shared.Models;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>();
        CreateMap<Entities.OrderDetail, OrderDetail>();
    }
}