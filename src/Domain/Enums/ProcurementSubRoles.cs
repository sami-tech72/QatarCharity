namespace Domain.Enums;

public static class ProcurementSubRoles
{
    public const string Manager = "ProcurementManager";
    public const string Officer = "ProcurementOfficer";
    public const string Viewer = "ProcurementViewer";

    public static readonly string[] All = [Manager, Officer, Viewer];
}
