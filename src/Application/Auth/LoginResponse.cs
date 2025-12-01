namespace Application.Auth;

public record LoginResponse(string Username, string DisplayName, string Role, string Token);
