using System.ComponentModel.DataAnnotations;

namespace Api.Models.Procurement;

public class AssignProcurementSubRoleRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public bool CanView { get; init; }

    public bool CanCreate { get; init; }

    public bool CanUpdate { get; init; }

    public bool CanDelete { get; init; }
}
