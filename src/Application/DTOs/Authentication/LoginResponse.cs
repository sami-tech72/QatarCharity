using System.Collections.Generic;

namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string Token,
    DateTime ExpiresAt,
    IEnumerable<string> ProcurementSubRoles);
