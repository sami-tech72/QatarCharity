namespace Application.DTOs.Authentication;

public class PortalUserResult
{
    public bool Success { get; init; }

    public string? UserId { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static PortalUserResult Ok(string userId) => new()
    {
        Success = true,
        UserId = userId,
    };

    public static PortalUserResult Fail(string errorCode, string errorMessage) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
    };
}
