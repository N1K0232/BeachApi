using Microsoft.AspNetCore.Identity;

namespace BeachApi.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string ProfilePhoto { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public Guid? TenantId { get; set; }

    public virtual Tenant Tenant { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}