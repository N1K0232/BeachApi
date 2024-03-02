using BeachApi.Shared.Models;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IAuthenticatedService
{
    Task<Result> AddProfilePhotoAsync(Stream stream, string fileName);

    Task<Result<User>> GetAsync();

    Task<Result<StreamFileContent>> GetProfilePhotoAsync();
}