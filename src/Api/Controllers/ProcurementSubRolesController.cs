using System.Linq;
using System.Security.Claims;
using Api.Models;
using Api.Models.Procurement;
using Application.DTOs;
using Application.Permissions;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/procurement/users/{userId}/sub-roles")]
[Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
public class ProcurementSubRolesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProcurementSubRolesController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProcurementSubRoleUpdateResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProcurementSubRoleUpdateResult>>> Assign(
        string userId,
        [FromBody] AssignProcurementSubRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("User not found.", errorCode: "user_not_found"));
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Procurement))
        {
            return NotFound(ApiResponse<object>.Fail("User is not a procurement account.", errorCode: "user_not_procurement"));
        }

        var permissions = new ProcurementPermissionSet(
            CanView: request.CanView,
            CanCreate: request.CanCreate,
            CanUpdate: request.CanUpdate,
            CanDelete: request.CanDelete);

        var claims = await _userManager.GetClaimsAsync(user);
        var existing = claims.FirstOrDefault(claim =>
            claim.Type == CustomClaimTypes.ProcurementSubRole &&
            claim.Value.Split('|', 2, System.StringSplitOptions.TrimEntries)[0]
                .Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            await _userManager.RemoveClaimAsync(user, existing);
        }

        await _userManager.AddClaimAsync(
            user,
            new Claim(CustomClaimTypes.ProcurementSubRole, ProcurementPermissionCalculator.ToClaimValue(request.Name, permissions)));

        var updatedClaims = await _userManager.GetClaimsAsync(user);
        var grants = ProcurementPermissionCalculator.ParseClaims(
            updatedClaims
                .Where(claim => claim.Type == CustomClaimTypes.ProcurementSubRole)
                .Select(claim => claim.Value));

        var response = new ProcurementSubRoleUpdateResult(
            user.Id,
            grants,
            ProcurementPermissionCalculator.CombineFor(grants));

        return Ok(ApiResponse<ProcurementSubRoleUpdateResult>.Ok(response, "Sub-role saved."));
    }
}

public record ProcurementSubRoleUpdateResult(
    string UserId,
    IReadOnlyCollection<ProcurementSubRoleGrant> SubRoles,
    ProcurementPermissionSet Permissions);
