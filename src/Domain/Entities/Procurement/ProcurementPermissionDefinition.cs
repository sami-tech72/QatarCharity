namespace Domain.Entities.Procurement;

public class ProcurementPermissionDefinition
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool DefaultRead { get; set; }

    public bool DefaultWrite { get; set; }

    public bool DefaultCreate { get; set; }

    public ICollection<ProcurementRolePermission> RolePermissions { get; set; } = new List<ProcurementRolePermission>();
}
