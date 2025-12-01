using Application.Auth;
using Microsoft.AspNetCore.Identity;
using Persistence.Identity;
using System.Linq;

namespace Infrastructure.Auth;

public class IdentityAuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            return null;
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return new LoginResponse(user.UserName!, user.DisplayName ?? user.UserName!, role, token);
    }
}
