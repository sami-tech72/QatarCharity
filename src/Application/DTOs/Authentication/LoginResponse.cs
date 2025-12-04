using System.Collections.Generic;
using Application.DTOs;

namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string Token,
    DateTime ExpiresAt,
    IReadOnlyCollection<string>? ProcurementSubRoles = null,
    ProcurementPermissionSet? ProcurementPermissions = null);
