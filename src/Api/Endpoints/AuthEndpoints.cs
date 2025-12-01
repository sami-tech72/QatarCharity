using Api.Contracts.Authentication;
using Application.Authentication;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder group)
    {
        var authGroup = group.MapGroup("/auth");

        authGroup.MapPost("/login", HandleLogin)
            .WithName("Login")
            .Produces<LoginResponse>();

        return group;
    }

    private static async Task<IResult> HandleLogin(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!signInResult.Succeeded)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var tokenResult = tokenService.CreateToken(user, roles);

        return Results.Ok(new LoginResponse(
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Role: roles.FirstOrDefault() ?? Roles.Supplier,
            Token: tokenResult.Token,
            ExpiresAt: tokenResult.ExpiresAt));
    }
}
