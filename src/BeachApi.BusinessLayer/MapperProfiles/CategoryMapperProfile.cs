using AutoMapper;
using BeachApi.Shared.Models;
using BeachApi.Shared.Models.Requests;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

public class CategoryMapperProfile : Profile
{
    public CategoryMapperProfile()
    {
        CreateMap<Entities.Category, Category>();
        CreateMap<SaveCategoryRequest, Entities.Category>();
    }
}