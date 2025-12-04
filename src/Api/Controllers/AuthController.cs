using System.Linq;
using System.Security.Claims;
using Api.Models;
using Application.Interfaces.Authentication;
using Application.DTOs.Authentication;
using Application.Permissions;
using Domain.Entities;
using Domain.Constants;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                "Invalid email or password.",
                errorCode: "auth_invalid_credentials"));
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                "Invalid email or password.",
                errorCode: "auth_invalid_credentials"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var tokenResult = _tokenService.CreateToken(user, roles, claims);

        var procurementSubRoles = GetProcurementSubRoles(claims, roles);
        var procurementPermissions = procurementSubRoles.Any()
            ? ProcurementPermissionCalculator.CombineFor(procurementSubRoles)
            : null;

        var response = new LoginResponse(
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Role: roles.FirstOrDefault() ?? Roles.Supplier,
            Token: tokenResult.Token,
            ExpiresAt: tokenResult.ExpiresAt,
            ProcurementSubRoles: procurementSubRoles,
            ProcurementPermissions: procurementPermissions);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    private static IReadOnlyCollection<string> GetProcurementSubRoles(
        IEnumerable<System.Security.Claims.Claim> claims,
        IEnumerable<string> roles)
    {
        if (!roles.Contains(Roles.Procurement))
        {
            return Array.Empty<string>();
        }

        return claims
            .Where(claim => claim.Type == CustomClaimTypes.ProcurementSubRole)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
