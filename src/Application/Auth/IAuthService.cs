namespace Application.Auth;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
