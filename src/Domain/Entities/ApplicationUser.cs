using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    [MaxLength(200)]
    public string? ProcurementSubRole { get; set; }

    public bool ProcurementCanCreate { get; set; }

    public bool ProcurementCanDelete { get; set; }

    public bool ProcurementCanView { get; set; }

    public bool ProcurementCanEdit { get; set; }
}
