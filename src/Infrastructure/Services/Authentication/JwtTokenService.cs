using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using Application.DTOs.Authentication;
using Application.Interfaces.Authentication;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Authentication;

public class JwtTokenService(IOptions<JwtSettings> settings) : IJwtTokenService
{
    private readonly JwtSettings _settings = settings.Value;

    public JwtTokenResult CreateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (!string.IsNullOrWhiteSpace(user.ProcurementSubRole))
        {
            claims.Add(new Claim("procurement_sub_role", user.ProcurementSubRole));
            claims.Add(new Claim("procurement_can_create", user.ProcurementCanCreate.ToString()));
            claims.Add(new Claim("procurement_can_delete", user.ProcurementCanDelete.ToString()));
            claims.Add(new Claim("procurement_can_view", user.ProcurementCanView.ToString()));
            claims.Add(new Claim("procurement_can_edit", user.ProcurementCanEdit.ToString()));
        }

        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials);

        var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(encodedToken, expires);
    }
}
