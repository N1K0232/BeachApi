using AutoMapper;
using BeachApi.Authentication.Entities;
using BeachApi.Shared.Models;

namespace BeachApi.BusinessLayer.MapperProfiles;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<ApplicationUser, User>();
    }
}