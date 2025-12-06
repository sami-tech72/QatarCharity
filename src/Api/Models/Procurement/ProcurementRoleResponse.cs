namespace Api.Models.Procurement;

public record ProcurementPermissionDto(
    string Menu,
    bool View,
    bool Edit,
    bool Create,
    bool Delete);

public record ProcurementSubRoleDto(
    string Name,
    int Users,
    IReadOnlyCollection<string> Avatars,
    int? ExtraUsers,
    string? Badge,
    IReadOnlyCollection<ProcurementPermissionDto> Permissions);

public record ProcurementRoleResponse(
    string MainRole,
    IReadOnlyCollection<ProcurementSubRoleDto> SubRoles,
    IReadOnlyCollection<ProcurementPermissionDto> MenuPermissions);
