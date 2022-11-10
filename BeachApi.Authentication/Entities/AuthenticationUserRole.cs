using Microsoft.AspNetCore.Identity;

namespace BeachApi.Authentication.Entities;

public class AuthenticationUserRole : IdentityUserRole<Guid>
{
    public virtual AuthenticationUser User { get; set; }

    public virtual AuthenticationRole Role { get; set; }
}