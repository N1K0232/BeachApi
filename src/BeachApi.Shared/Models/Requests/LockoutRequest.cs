namespace BeachApi.Shared.Models.Requests;

public class LockoutRequest
{
    public Guid UserId { get; set; }

    public DateTimeOffset LockoutEnd { get; set; }
}