using System;
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
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetUsers([FromQuery] UserQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var searchTerm = query.Search?.Trim();

        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var loweredSearch = searchTerm.ToLower();

            usersQuery = usersQuery.Where(user =>
                (user.DisplayName ?? string.Empty).ToLower().Contains(loweredSearch) ||
                (user.Email ?? string.Empty).ToLower().Contains(loweredSearch));
        }

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var pagedResult = new PagedResult<UserResponse>(
            response,
            totalCount,
            pageNumber,
            pageSize);

        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(pagedResult, "Users retrieved successfully."));
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(string id, UpdateUserRequest request)
    {
        if (!Roles.All.Contains(request.Role))
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Invalid role provided.",
                errorCode: "users_invalid_role"));
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound(ApiResponse<UserResponse>.Fail(
                "User not found.",
                errorCode: "users_not_found"));
        }

        var otherUserWithEmail = await _userManager.FindByEmailAsync(request.Email);

        if (otherUserWithEmail is not null && otherUserWithEmail.Id != user.Id)
        {
            return Conflict(ApiResponse<UserResponse>.Fail(
                "A user with this email already exists.",
                errorCode: "users_duplicate_email"));
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Unable to update user.",
                errorCode: "users_update_failed",
                details: BuildErrorDetails(updateResult)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(request.Role))
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                return BadRequest(ApiResponse<UserResponse>.Fail(
                    "Unable to update user role.",
                    errorCode: "users_role_remove_failed",
                    details: BuildErrorDetails(removeResult)));
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);

            if (!addResult.Succeeded)
            {
                return BadRequest(ApiResponse<UserResponse>.Fail(
                    "Unable to assign role to the user.",
                    errorCode: "users_role_assignment_failed",
                    details: BuildErrorDetails(addResult)));
            }
        }

        var response = new UserResponse(
            Id: user.Id,
            DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Role: request.Role);

        return Ok(ApiResponse<UserResponse>.Ok(response, "User updated successfully."));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail(
                "User not found.",
                errorCode: "users_not_found"));
        }

        var deleteResult = await _userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Unable to delete user.",
                errorCode: "users_delete_failed",
                details: BuildErrorDetails(deleteResult)));
        }

        return Ok(ApiResponse<object>.Ok(null, "User deleted successfully."));
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
