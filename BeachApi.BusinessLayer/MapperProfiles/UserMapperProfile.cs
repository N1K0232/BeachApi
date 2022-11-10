using AutoMapper;
using BeachApi.Authentication.Entities;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;

namespace BeachApi.BusinessLayer.MapperProfiles;

internal class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<AuthenticationUser, User>();
        CreateMap<RegisterRequest, AuthenticationUser>();
    }
}