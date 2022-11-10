using AutoMapper;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.MapperProfiles;

internal class InvoiceMapperProfile : Profile
{
    public InvoiceMapperProfile()
    {
        CreateMap<Entities.Invoice, Invoice>();
        CreateMap<SaveInvoiceRequest, Entities.Invoice>();
    }
}