using System.Linq;
using Api.Models;
using Api.Models.Users;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetUsers()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var response = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            response.Add(new UserResponse(
                Id: user.Id,
                DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                Role: roles.FirstOrDefault() ?? Roles.Supplier));
        }

        return Ok(ApiResponse<IEnumerable<UserResponse>>.Ok(response, "Users retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(CreateUserRequest request)
    {
        if (!Roles.All.Contains(request.Role))
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Invalid role provided.",
                errorCode: "users_invalid_role"));
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return Conflict(ApiResponse<UserResponse>.Fail(
                "A user with this email already exists.",
                errorCode: "users_duplicate_email"));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Unable to create user.",
                errorCode: "users_create_failed",
                details: BuildErrorDetails(createResult)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Unable to assign role to the user.",
                errorCode: "users_role_assignment_failed",
                details: BuildErrorDetails(roleResult)));
        }

        var response = new UserResponse(
            Id: user.Id,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Role: request.Role);

        return Ok(ApiResponse<UserResponse>.Ok(response, "User created successfully."));
    }

    private static Dictionary<string, object?> BuildErrorDetails(IdentityResult result)
    {
        return new Dictionary<string, object?>
        {
            ["errors"] = result.Errors.Select(error => new
            {
                error.Code,
                error.Description
            }).ToArray()
        };
    }
}
