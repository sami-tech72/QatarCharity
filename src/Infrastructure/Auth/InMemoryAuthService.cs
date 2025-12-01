using Application.Auth;
using Domain.Users;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth;

public class InMemoryAuthService : IAuthService
{
    private readonly IReadOnlyCollection<UserAccount> _users;

    public InMemoryAuthService(IOptions<AuthOptions> options)
    {
        _users = options.Value.Users
            .Select(user => new UserAccount
            {
                Username = user.Username,
                Password = user.Password,
                DisplayName = user.DisplayName,
                Role = user.Role,
            })
            .ToArray();
    }

    public Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(account =>
            string.Equals(account.Username, request.Username, StringComparison.OrdinalIgnoreCase));

        if (user is null || !string.Equals(user.Password, request.Password))
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var response = new LoginResponse(user.Username, user.DisplayName, user.Role, token);

        return Task.FromResult<LoginResponse?>(response);
    }
}
