namespace Domain.Authorization;

public static class ProcurementPermissions
{
    public const string View = "Procurement.View";
    public const string Create = "Procurement.Create";
    public const string Update = "Procurement.Update";
    public const string Delete = "Procurement.Delete";

    public static readonly string[] All = [View, Create, Update, Delete];
}
