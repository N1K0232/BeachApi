using AutoMapper;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

internal class CategoryMapperProfile : Profile
{
    public CategoryMapperProfile()
    {
        CreateMap<Entities.Category, Category>();
        CreateMap<SaveCategoryRequest, Entities.Category>();
    }
}