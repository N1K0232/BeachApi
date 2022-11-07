namespace BeachApi.SharedServices;

public interface IUserClaimService
{
    Guid GetId();

    string GetUserName();
}