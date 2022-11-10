using AutoMapper;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

internal class SupplierMapperProfile : Profile
{
    public SupplierMapperProfile()
    {
        CreateMap<Entities.Supplier, Supplier>();
        CreateMap<SaveSupplierRequest, Entities.Supplier>();
    }
}