namespace Domain.Enums;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Procurement = "Procurement";
    public const string Supplier = "Supplier";

    public static readonly string[] All = [Admin, Procurement, Supplier];
}
