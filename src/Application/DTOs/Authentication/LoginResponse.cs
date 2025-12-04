namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string? ProcurementSubRole,
    bool ProcurementCanCreate,
    bool ProcurementCanDelete,
    bool ProcurementCanView,
    bool ProcurementCanEdit,
    string Token,
    DateTime ExpiresAt);
