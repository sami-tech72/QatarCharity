using Microsoft.AspNetCore.Identity;

namespace Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
