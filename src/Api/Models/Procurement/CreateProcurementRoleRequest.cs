using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Procurement;

public record CreateProcurementRoleRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<ProcurementPermission>? Permissions { get; init; }
}
