namespace BeachApi.Shared.Models.Requests;

public record class RefreshTokenRequest(string AccessToken, string RefreshToken);