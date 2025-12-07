using Domain.Entities.Procurement;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public int? ProcurementRoleTemplateId { get; set; }

    public ProcurementRoleTemplate? ProcurementRoleTemplate { get; set; }
}
