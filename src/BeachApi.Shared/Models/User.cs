using BeachApi.Shared.Models.Common;

namespace BeachApi.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;
}