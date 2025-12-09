using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Api.Models;
using Application.Interfaces.Authentication;
using Application.DTOs.Authentication;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Identity;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _tokenService;
    private readonly AppDbContext _dbContext;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService tokenService,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
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
        var response = await CreateLoginResponseAsync(user, roles);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid or expired token.", errorCode: "auth_invalid_token"));
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("User not found.", errorCode: "auth_invalid_token"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = await CreateLoginResponseAsync(user, roles);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Session verified."));
    }

    private async Task<LoginResponse> CreateLoginResponseAsync(ApplicationUser user, IList<string> roles)
    {
        var procurementRole = await BuildProcurementRoleAsync(user, roles);

        var tokenResult = _tokenService.CreateToken(
            new AuthenticatedUser(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.DisplayName),
            roles,
            BuildProcurementClaims(procurementRole));

        return new LoginResponse(
            UserId: user.Id,
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Role: roles.FirstOrDefault() ?? Roles.Supplier,
            Token: tokenResult.Token,
            ExpiresAt: tokenResult.ExpiresAt,
            ProcurementRole: procurementRole);
    }

    private static IEnumerable<Claim>? BuildProcurementClaims(ProcurementUserRoleDto? procurementRole)
    {
        if (procurementRole is null)
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new("procurement_role_id", procurementRole.Id.ToString())
        };

        claims.AddRange(procurementRole.Permissions.Select(permission =>
            new Claim($"procurement_permission:{permission.Name}",
                string.Join(",", new[]
                {
                    permission.Actions.Read ? "read" : null,
                    permission.Actions.Write ? "write" : null,
                    permission.Actions.Create ? "create" : null,
                }.Where(action => action is not null)!))));

        return claims;
    }

    private async Task<ProcurementUserRoleDto?> BuildProcurementRoleAsync(ApplicationUser user, IList<string> roles)
    {
        if (user.ProcurementRoleTemplateId is null ||
            !roles.Any(role => string.Equals(role, Roles.Procurement, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var procurementRoleLookup = await LoadProcurementRoleLookupAsync();

        if (!procurementRoleLookup.TryGetValue(user.ProcurementRoleTemplateId.Value, out var template))
        {
            return null;
        }

        var permissions = template.Permissions
            .Select(permission => new ProcurementPermissionDto(
                permission.ProcurementPermissionDefinition.Name,
                new ProcurementPermissionActionsDto(permission.CanRead, permission.CanWrite, permission.CanCreate)))
            .ToList();

        return new ProcurementUserRoleDto(template.Id, template.Name, permissions);
    }

    private Task<Dictionary<int, Domain.Entities.Procurement.ProcurementRoleTemplate>> LoadProcurementRoleLookupAsync()
    {
        return _dbContext.ProcurementRoleTemplates
            .Include(template => template.Permissions)
            .ThenInclude(permission => permission.ProcurementPermissionDefinition)
            .AsNoTracking()
            .ToDictionaryAsync(template => template.Id);
    }
}
