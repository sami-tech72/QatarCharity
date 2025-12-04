namespace Domain.Enums;

public static class ProcurementSubRoles
{
    public const string Lead = "Lead";
    public const string Sourcing = "Sourcing";
    public const string Reporting = "Reporting";

    public static readonly string[] All = [Lead, Sourcing, Reporting];
}
