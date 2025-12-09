namespace Application.DTOs.Authentication;

public record AuthenticatedUser(string Id, string UserName, string Email, string? DisplayName);
