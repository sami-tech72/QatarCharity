using System.Collections.Generic;
using System.Security.Claims;
using Application.DTOs.Authentication;

namespace Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(
        AuthenticatedUser user,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null);
}
