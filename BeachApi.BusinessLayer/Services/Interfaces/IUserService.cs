using BeachApi.Shared.Requests;
using BeachApi.Shared.Responses;

namespace BeachApi.BusinessLayer.Services.Interfaces;

public interface IUserService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}