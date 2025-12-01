namespace Api.Contracts.Authentication;

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string Token,
    DateTime ExpiresAt);
