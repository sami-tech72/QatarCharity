namespace Domain.Enums;

public static class ProcurementSubRoles
{
    public const string Viewer = "ProcurementViewer";
    public const string Contributor = "ProcurementContributor";
    public const string Manager = "ProcurementManager";

    public static readonly string[] All = [Viewer, Contributor, Manager];
}
