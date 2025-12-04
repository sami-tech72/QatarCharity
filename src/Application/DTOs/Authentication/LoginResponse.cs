using System.Collections.Generic;

namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    IEnumerable<string> Roles,
    IEnumerable<string> SubRoles,
    string Token,
    DateTime ExpiresAt);
