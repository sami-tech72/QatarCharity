using Api.Models;
using Application.Interfaces.Authentication;
using Application.DTOs.Authentication;
using Domain.Entities;
using Domain.Enums;
using System.Linq;
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
        var primaryRole = roles.Contains(Roles.Procurement)
            ? Roles.Procurement
            : roles.FirstOrDefault() ?? Roles.Supplier;
        var subRoles = roles.Where(role => role != primaryRole).ToArray();
        var tokenResult = _tokenService.CreateToken(user, roles);

        var response = new LoginResponse(
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Role: primaryRole,
            Roles: roles,
            SubRoles: subRoles,
            Token: tokenResult.Token,
            ExpiresAt: tokenResult.ExpiresAt);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }
}
