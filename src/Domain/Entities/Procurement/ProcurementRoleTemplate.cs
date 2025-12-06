namespace Domain.Entities.Procurement;

public class ProcurementRoleTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TotalUsers { get; set; }

    public int NewUsers { get; set; }

    public int? ExtraCount { get; set; }

    public ICollection<ProcurementRoleAvatar> Avatars { get; set; } = new List<ProcurementRoleAvatar>();

    public ICollection<ProcurementRolePermission> Permissions { get; set; } = new List<ProcurementRolePermission>();
}
