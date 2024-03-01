using BeachApi.Shared.Models;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;
public interface IAuthenticatedService
{
    Task<Result<User>> GetAsync();
}