using Application.Auth;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Auth;

public class SqlAuthService : IAuthService
{
    private readonly AuthDbContext _dbContext;

    public SqlAuthService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var user = await _dbContext.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!string.Equals(user.Password, request.Password))
        {
            return null;
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return new LoginResponse(user.Username, user.DisplayName, user.Role, token);
    }
}
