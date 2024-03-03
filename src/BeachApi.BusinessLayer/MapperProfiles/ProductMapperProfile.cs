using AutoMapper;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Requests;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<Entities.Product, Product>()
            .ForMember(p => p.Category, options => options.MapFrom(p => p.Category.Name));

        CreateMap<SaveProductRequest, Entities.Product>();
    }
}