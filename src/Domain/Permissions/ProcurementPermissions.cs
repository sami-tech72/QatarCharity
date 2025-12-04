using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Permissions;

public static class ProcurementPermissions
{
    public const string Dashboard = "dashboard";
    public const string RfxManagement = "rfx-management";
    public const string RfxManagementCreate = "rfx-management:create";
    public const string BidEvaluation = "bid-evaluation";
    public const string TenderCommittee = "tender-committee";
    public const string ContractManagement = "contract-management";
    public const string SupplierPerformance = "supplier-performance";
    public const string ReportsAnalytics = "reports-analytics";
    public const string Settings = "settings";

    private static readonly IReadOnlyDictionary<string, string[]> SubRolePermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [ProcurementSubRoles.Lead] =
            [
                Dashboard,
                RfxManagement,
                RfxManagementCreate,
                BidEvaluation,
                TenderCommittee,
                ContractManagement,
                SupplierPerformance,
                ReportsAnalytics,
                Settings,
            ],
            [ProcurementSubRoles.Sourcing] =
            [
                Dashboard,
                RfxManagement,
                RfxManagementCreate,
                BidEvaluation,
                TenderCommittee,
                ContractManagement,
            ],
            [ProcurementSubRoles.Reporting] =
            [
                Dashboard,
                SupplierPerformance,
                ReportsAnalytics,
            ],
        };

    public static IReadOnlyCollection<string> ForSubRole(string? subRole)
    {
        if (string.IsNullOrWhiteSpace(subRole))
        {
            return Array.Empty<string>();
        }

        return SubRolePermissions.TryGetValue(subRole, out var permissions)
            ? permissions
            : Array.Empty<string>();
    }
}
