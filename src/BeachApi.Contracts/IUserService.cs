namespace BeachApi.Contracts;

public interface IUserService
{
    Guid GetId();

    Guid GetTenantId();

    string GetUserName();
}