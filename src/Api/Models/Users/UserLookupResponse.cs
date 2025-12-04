using System.Collections.Generic;

namespace Api.Models.Users;

public record UserLookupResponse(
    string Id,
    string DisplayName,
    string Email,
    string Role,
    IEnumerable<string> ProcurementSubRoles);
