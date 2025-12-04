namespace Domain.Enums;

public static class ProcurementSubRoles
{
    public const string Dashboard = "Dashboard";
    public const string RfxManagement = "RFx Management";
    public const string BidEvaluation = "Bid Evaluation";
    public const string TenderCommittee = "Tender Committee";
    public const string ContractManagement = "Contract Management";
    public const string SupplierPerformance = "Supplier Performance";
    public const string ReportsAnalytics = "Reports & Analytics";
    public const string Settings = "Settings";

    public static readonly string[] All =
    [
        Dashboard,
        RfxManagement,
        BidEvaluation,
        TenderCommittee,
        ContractManagement,
        SupplierPerformance,
        ReportsAnalytics,
        Settings
    ];
}
