namespace BeachApi.Shared.Responses;

public class LoginResponse
{
    public string AccessToken { get; init; }

    public string RefreshToken { get; init; }
}