namespace Application.Authentication;

public record JwtTokenResult(string Token, DateTime ExpiresAt);
