using Application.DTOs.Authentication;

namespace Api.Models.Users;

public record UserResponse(
    string Id,
    string DisplayName,
    string Email,
    string Role,
    ProcurementUserRoleDto? ProcurementRole);
