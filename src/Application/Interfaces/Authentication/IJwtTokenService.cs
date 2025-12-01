using System.Collections.Generic;
using Application.DTOs.Authentication;
using Domain.Entities;

namespace Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
