using BeachApi.Shared.Models.Requests;
using BeachApi.Shared.Models.Responses;
using OperationResults;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result> DeleteAsync(Guid userId);

    Task<Result> LockoutAsync(LockoutRequest request);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);
}