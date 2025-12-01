using Application.DTOs.Authentication;
using Domain.Entities;

namespace Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
