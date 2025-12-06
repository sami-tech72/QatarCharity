namespace Domain.Entities.Procurement;

public class ProcurementRolePermission
{
    public int Id { get; set; }

    public int ProcurementRoleTemplateId { get; set; }

    public ProcurementRoleTemplate ProcurementRoleTemplate { get; set; } = null!;

    public int ProcurementPermissionDefinitionId { get; set; }

    public ProcurementPermissionDefinition ProcurementPermissionDefinition { get; set; } = null!;

    public bool CanRead { get; set; }

    public bool CanWrite { get; set; }

    public bool CanCreate { get; set; }
}
