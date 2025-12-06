using System.Collections.Generic;
using Api.Models;
using Api.Models.Procurement;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/procurement/roles")]
[Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
public class ProcurementRolesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRolesResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<ProcurementRolesResponse>> GetProcurementRoles()
    {
        var defaultPermissions = new List<ProcurementPermission>
        {
            new("User Management", new ProcurementPermissionActions(true, true, true)),
            new("Content Management", new ProcurementPermissionActions(true, true, true)),
            new("Disputes Management", new ProcurementPermissionActions(true, true, true)),
            new("Database Management", new ProcurementPermissionActions(true, true, true)),
            new("Finance Management", new ProcurementPermissionActions(true, true, true)),
            new("Reporting", new ProcurementPermissionActions(true, true, true)),
            new("API Control", new ProcurementPermissionActions(true, true, true)),
            new("Repository Management", new ProcurementPermissionActions(true, true, true)),
            new("Payroll", new ProcurementPermissionActions(true, true, true)),
        };

        var response = new ProcurementRolesResponse(
            MainRole: Roles.Procurement,
            SubRoles: new List<ProcurementSubRole>
            {
                new(
                    Name: "Administrator",
                    Description: "Best for business owners and company administrators",
                    TotalUsers: 4,
                    NewUsers: 2,
                    Avatars: new[] { "300-6.jpg", "300-5.jpg", "300-11.jpg", "300-3.jpg" },
                    ExtraCount: null,
                    Permissions: ClonePermissions(defaultPermissions)),
                new(
                    Name: "Manager",
                    Description: "Best for team leads to manage permissions",
                    TotalUsers: 5,
                    NewUsers: 2,
                    Avatars: new[] { "300-14.jpg", "300-2.jpg", "300-7.jpg", "300-8.jpg" },
                    ExtraCount: 1,
                    Permissions: ClonePermissions(defaultPermissions, create: false)),
                new(
                    Name: "Users",
                    Description: "Best for standard users who need access to all standard features.",
                    TotalUsers: 8,
                    NewUsers: 4,
                    Avatars: new[] { "300-9.jpg", "300-10.jpg", "300-12.jpg", "300-13.jpg" },
                    ExtraCount: 2,
                    Permissions: ClonePermissions(defaultPermissions, write: false, create: false)),
                new(
                    Name: "Support",
                    Description: "Best for employees who regularly refund payments",
                    TotalUsers: 3,
                    NewUsers: 2,
                    Avatars: new[] { "300-4.jpg", "300-1.jpg", "300-19.jpg" },
                    ExtraCount: null,
                    Permissions: ClonePermissions(defaultPermissions, write: false, create: false)),
                new(
                    Name: "Restricted User",
                    Description: "Best for people who need restricted access to sensitive data",
                    TotalUsers: 4,
                    NewUsers: 1,
                    Avatars: new[] { "300-21.jpg", "300-23.jpg", "300-24.jpg", "300-25.jpg" },
                    ExtraCount: null,
                    Permissions: ClonePermissions(defaultPermissions, read: true, write: false, create: false)),
            },
            DefaultPermissions: defaultPermissions);

        return Ok(ApiResponse<ProcurementRolesResponse>.Ok(response, "Procurement roles retrieved successfully."));
    }

    private static IReadOnlyList<ProcurementPermission> ClonePermissions(
        IEnumerable<ProcurementPermission> basePermissions,
        bool? read = null,
        bool? write = null,
        bool? create = null)
    {
        var permissions = new List<ProcurementPermission>();

        foreach (var permission in basePermissions)
        {
            permissions.Add(permission with
            {
                Actions = new ProcurementPermissionActions(
                    read ?? permission.Actions.Read,
                    write ?? permission.Actions.Write,
                    create ?? permission.Actions.Create)
            });
        }

        return permissions;
    }
}
