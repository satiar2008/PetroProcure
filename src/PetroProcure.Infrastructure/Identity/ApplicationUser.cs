using Microsoft.AspNetCore.Identity;

namespace PetroProcure.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? UserProfileId { get; set; }
}
