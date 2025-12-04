using System;
using System.Collections.Generic;
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
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
            var primaryRole = roles.Contains(Roles.Procurement)
                ? Roles.Procurement
                : roles.FirstOrDefault() ?? Roles.Supplier;
            var subRoles = roles
                .Where(role => role != primaryRole && ProcurementSubRoles.All.Contains(role))
                .ToArray();

            response.Add(new UserResponse(
                Id: user.Id,
                DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                Role: primaryRole,
                SubRoles: subRoles));
        }

        var pagedResult = new PagedResult<UserResponse>(
            response,
            totalCount,
            pageNumber,
            pageSize);

        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(pagedResult, "Users retrieved successfully."));
    }

    [HttpGet("lookup")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserLookupResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserLookupResponse>>>> GetUserLookup([FromQuery] string? search)
    {
        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var loweredSearch = search.Trim().ToLowerInvariant();

            usersQuery = usersQuery.Where(user =>
                (user.DisplayName ?? string.Empty).ToLower().Contains(loweredSearch) ||
                (user.Email ?? string.Empty).ToLower().Contains(loweredSearch));
        }

        var users = await usersQuery
            .OrderBy(user => user.DisplayName ?? user.Email ?? user.UserName)
            .Take(100)
            .ToListAsync();

        var responses = new List<UserLookupResponse>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.Contains(Roles.Procurement)
                ? Roles.Procurement
                : roles.FirstOrDefault() ?? Roles.Supplier;
            var subRoles = roles
                .Where(role => role != primaryRole && ProcurementSubRoles.All.Contains(role))
                .ToArray();

            responses.Add(new UserLookupResponse(
                Id: user.Id,
                DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                Role: primaryRole,
                SubRoles: subRoles));
        }

        return Ok(ApiResponse<IEnumerable<UserLookupResponse>>.Ok(responses, "Users retrieved successfully."));
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

        if (request.Role != Roles.Procurement && request.SubRoles?.Any() == true)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Sub-roles are only supported for Procurement users.",
                errorCode: "users_invalid_subroles"));
        }

        var invalidSubRoles = GetInvalidSubRoles(request.SubRoles);

        if (invalidSubRoles.Any())
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Invalid procurement sub-roles provided.",
                errorCode: "users_invalid_subroles",
                details: new Dictionary<string, object?> { ["invalidSubRoles"] = invalidSubRoles }));
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

        var desiredRoles = BuildDesiredRoles(request.Role, request.SubRoles);
        var ensureRolesResult = await EnsureRolesExist(desiredRoles);

        if (!ensureRolesResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Unable to assign role to the user.",
                errorCode: "users_role_assignment_failed",
                details: BuildErrorDetails(ensureRolesResult)));
        }

        var roleResult = await _userManager.AddToRolesAsync(user, desiredRoles);

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
            Role: request.Role,
            SubRoles: desiredRoles.Where(role => role != request.Role));

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

        if (request.Role != Roles.Procurement && request.SubRoles?.Any() == true)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Sub-roles are only supported for Procurement users.",
                errorCode: "users_invalid_subroles"));
        }

        var invalidSubRoles = GetInvalidSubRoles(request.SubRoles);

        if (invalidSubRoles.Any())
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Invalid procurement sub-roles provided.",
                errorCode: "users_invalid_subroles",
                details: new Dictionary<string, object?> { ["invalidSubRoles"] = invalidSubRoles }));
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

        var desiredRoles = BuildDesiredRoles(request.Role, request.SubRoles);
        var ensureRolesResult = await EnsureRolesExist(desiredRoles);

        if (!ensureRolesResult.Succeeded)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(
                "Unable to ensure roles exist.",
                errorCode: "users_role_assignment_failed",
                details: BuildErrorDetails(ensureRolesResult)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(desiredRoles).ToArray();
        var rolesToAdd = desiredRoles.Except(currentRoles).ToArray();

        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (!removeResult.Succeeded)
            {
                return BadRequest(ApiResponse<UserResponse>.Fail(
                    "Unable to update user role.",
                    errorCode: "users_role_remove_failed",
                    details: BuildErrorDetails(removeResult)));
            }
        }

        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

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
            Role: request.Role,
            SubRoles: desiredRoles.Where(role => role != request.Role));

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

    private static IEnumerable<string> BuildDesiredRoles(string primaryRole, IEnumerable<string>? subRoles)
    {
        if (primaryRole != Roles.Procurement)
        {
            return [primaryRole];
        }

        var validSubRoles = (subRoles ?? Array.Empty<string>())
            .Where(role => ProcurementSubRoles.All.Contains(role))
            .Distinct()
            .ToArray();

        return [primaryRole, .. validSubRoles];
    }

    private async Task<IdentityResult> EnsureRolesExist(IEnumerable<string> roles)
    {
        var missingRoles = new List<IdentityRole>();

        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            missingRoles.Add(new IdentityRole(role));
        }

        if (!missingRoles.Any())
        {
            return IdentityResult.Success;
        }

        var errors = new List<IdentityError>();

        foreach (var role in missingRoles)
        {
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Any() ? IdentityResult.Failed([.. errors]) : IdentityResult.Success;
    }

    private static string[] GetInvalidSubRoles(IEnumerable<string>? subRoles)
    {
        return (subRoles ?? Array.Empty<string>())
            .Where(subRole => !ProcurementSubRoles.All.Contains(subRole))
            .Distinct()
            .ToArray();
    }
}
