using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    /// <summary>
    /// Comma-separated list of Procurement sub-roles assigned to the user. Applies only when the user's main role is Procurement.
    /// </summary>
    public string? ProcurementSubRoles { get; set; }
}
