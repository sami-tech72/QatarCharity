namespace Domain.Enums;

public static class SupplierStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string OnHold = "On Hold";

    public static readonly string[] All = [Pending, Approved, OnHold];
}
