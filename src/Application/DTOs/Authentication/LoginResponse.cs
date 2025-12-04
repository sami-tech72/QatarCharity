namespace Application.DTOs.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string? ProcurementRole,
    string Token,
    DateTime ExpiresAt);
