namespace Api.Models.Procurement;

public record ProcurementRolesResponse(
    string MainRole,
    IReadOnlyList<ProcurementSubRole> SubRoles,
    IReadOnlyList<ProcurementPermission> DefaultPermissions);

public record ProcurementSubRole(
    int Id,
    string Name,
    string Description,
    int TotalUsers,
    int NewUsers,
    IReadOnlyList<string> Avatars,
    int? ExtraCount,
    IReadOnlyList<ProcurementPermission> Permissions);

public record ProcurementPermission(string Name, ProcurementPermissionActions Actions);

public record ProcurementPermissionActions(bool Read, bool Write, bool Create);
