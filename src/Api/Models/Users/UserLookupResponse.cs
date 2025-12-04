namespace Api.Models.Users;

public record UserLookupResponse(
    string Id,
    string DisplayName,
    string Email,
    string Role,
    string? ProcurementSubRole,
    bool ProcurementCanCreate,
    bool ProcurementCanDelete,
    bool ProcurementCanView,
    bool ProcurementCanEdit);
