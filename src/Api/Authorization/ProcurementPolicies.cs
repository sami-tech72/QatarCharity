namespace Api.Authorization;

public static class ProcurementPolicies
{
    public const string RolesPermissionsRead = "procurement_permission:Roles & Permissions:read";
    public const string RolesPermissionsWrite = "procurement_permission:Roles & Permissions:write";
    public const string RolesPermissionsCreate = "procurement_permission:Roles & Permissions:create";

    public const string RfxManagementRead = "procurement_permission:RFx Management:read";
    public const string RfxManagementWrite = "procurement_permission:RFx Management:write";
    public const string RfxManagementCreate = "procurement_permission:RFx Management:create";

    public const string BidEvaluationRead = "procurement_permission:Bid Evaluation:read";
    public const string BidEvaluationWrite = "procurement_permission:Bid Evaluation:write";
    public const string BidEvaluationCreate = "procurement_permission:Bid Evaluation:create";

    public const string ContractManagementRead = "procurement_permission:Contract Management:read";
    public const string ContractManagementWrite = "procurement_permission:Contract Management:write";
    public const string ContractManagementCreate = "procurement_permission:Contract Management:create";

    public const string SupplierPerformanceRead = "procurement_permission:Supplier Performance:read";
    public const string SupplierPerformanceWrite = "procurement_permission:Supplier Performance:write";
    public const string SupplierPerformanceCreate = "procurement_permission:Supplier Performance:create";

    public const string ReportsAnalyticsRead = "procurement_permission:Reports & Analytics:read";
    public const string ReportsAnalyticsWrite = "procurement_permission:Reports & Analytics:write";
    public const string ReportsAnalyticsCreate = "procurement_permission:Reports & Analytics:create";

    public static IEnumerable<(string Permission, string Action, string PolicyName)> All
    {
        get
        {
            yield return ("Roles & Permissions", "read", RolesPermissionsRead);
            yield return ("Roles & Permissions", "write", RolesPermissionsWrite);
            yield return ("Roles & Permissions", "create", RolesPermissionsCreate);

            yield return ("RFx Management", "read", RfxManagementRead);
            yield return ("RFx Management", "write", RfxManagementWrite);
            yield return ("RFx Management", "create", RfxManagementCreate);

            yield return ("Bid Evaluation", "read", BidEvaluationRead);
            yield return ("Bid Evaluation", "write", BidEvaluationWrite);
            yield return ("Bid Evaluation", "create", BidEvaluationCreate);

            yield return ("Contract Management", "read", ContractManagementRead);
            yield return ("Contract Management", "write", ContractManagementWrite);
            yield return ("Contract Management", "create", ContractManagementCreate);

            yield return ("Supplier Performance", "read", SupplierPerformanceRead);
            yield return ("Supplier Performance", "write", SupplierPerformanceWrite);
            yield return ("Supplier Performance", "create", SupplierPerformanceCreate);

            yield return ("Reports & Analytics", "read", ReportsAnalyticsRead);
            yield return ("Reports & Analytics", "write", ReportsAnalyticsWrite);
            yield return ("Reports & Analytics", "create", ReportsAnalyticsCreate);
        }
    }
}
