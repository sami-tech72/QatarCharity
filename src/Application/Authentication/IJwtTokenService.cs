using Domain.Entities;

namespace Application.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
