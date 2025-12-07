namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string Token,
    DateTime ExpiresAt,
    ProcurementUserRoleDto? ProcurementRole);

public record ProcurementUserRoleDto(
    int Id,
    string Name,
    IReadOnlyList<ProcurementPermissionDto> Permissions);

public record ProcurementPermissionDto(string Name, ProcurementPermissionActionsDto Actions);

public record ProcurementPermissionActionsDto(bool Read, bool Write, bool Create);
