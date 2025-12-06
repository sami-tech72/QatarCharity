namespace Domain.Entities.Procurement;

public class ProcurementRoleAvatar
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int ProcurementRoleTemplateId { get; set; }

    public ProcurementRoleTemplate ProcurementRoleTemplate { get; set; } = null!;
}
