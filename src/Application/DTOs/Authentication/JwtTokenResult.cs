namespace Application.DTOs.Authentication;

public record JwtTokenResult(string Token, DateTime ExpiresAt);
