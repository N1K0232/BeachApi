using BeachApi.Shared.Models;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IImageService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Image>> GetAsync(Guid id);

    Task<Result<IEnumerable<Image>>> GetListAsync();

    Task<Result<StreamFileContent>> ReadAsync(Guid id);

    Task<Result<Image>> UploadAsync(StreamFileContent file, string description);
}