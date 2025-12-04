using System;
using System.Linq;
using Api.Models;
using Domain.Authorization;
using Application.Interfaces.Authentication;
using Application.DTOs.Authentication;
using Domain.Entities;
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
        var procurementSubRoles = await _userManager.GetClaimsAsync(user);
        var filteredSubRoles = procurementSubRoles
            .Where(claim => claim.Type == CustomClaimTypes.ProcurementSubRole)
            .Select(claim => claim.Value)
            .Distinct()
            .ToArray();

        var effectiveProcurementSubRoles = filteredSubRoles.Length > 0
            ? filteredSubRoles
            : new[] { ProcurementSubRoles.Viewer };

        var procurementAccess = roles.Contains(Roles.Procurement)
            ? new ProcurementAccess(
                SubRoles: effectiveProcurementSubRoles,
                Permissions: ProcurementSubRoles.PermissionsFor(effectiveProcurementSubRoles))
            : null;

        var additionalClaims = procurementAccess is null
            ? Array.Empty<System.Security.Claims.Claim>()
            : procurementAccess.SubRoles.Select(subRole => new System.Security.Claims.Claim(CustomClaimTypes.ProcurementSubRole, subRole));

        var tokenResult = _tokenService.CreateToken(user, roles, additionalClaims);

        var response = new LoginResponse(
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Role: roles.FirstOrDefault() ?? Roles.Supplier,
            Token: tokenResult.Token,
            ExpiresAt: tokenResult.ExpiresAt,
            ProcurementAccess: procurementAccess);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }
}
