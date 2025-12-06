using System.Collections.Generic;
using System.Linq;
using Api.Models;
using Api.Models.Procurement;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/procurement/roles")]
[Authorize(Roles = Roles.Procurement)]
public class ProcurementRolesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRoleResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<ProcurementRoleResponse>> GetRoles()
    {
        var permissions = new List<ProcurementPermissionDto>
        {
            new("Supplier Management", view: true, edit: true, create: true, delete: true),
            new("RFx Management", view: true, edit: true, create: true, delete: false),
            new("Purchase Orders", view: true, edit: true, create: true, delete: false),
            new("Contracts", view: true, edit: true, create: false, delete: false),
            new("Invoices", view: true, edit: false, create: false, delete: false),
            new("Reports", view: true, edit: false, create: false, delete: false),
            new("Settings", view: true, edit: true, create: false, delete: false),
        };

        var subRoles = new List<ProcurementSubRoleDto>
        {
            new(
                Name: "Procurement Admin",
                Users: 6,
                Avatars: ["AN", "MT", "CR", "HD"],
                ExtraUsers: 2,
                Badge: "Default",
                Permissions: permissions
            ),
            new(
                Name: "Category Manager",
                Users: 5,
                Avatars: ["LS", "BK", "AO", "TT"],
                ExtraUsers: 1,
                Badge: null,
                Permissions: permissions
                    .Select(p => p with { Delete = false })
                    .ToList()
            ),
            new(
                Name: "Sourcing Specialist",
                Users: 4,
                Avatars: ["GM", "ID", "RS", "LP"],
                ExtraUsers: 0,
                Badge: null,
                Permissions: permissions
                    .Select(p => p with { Edit = p.Edit && p.Menu != "Settings", Delete = false })
                    .ToList()
            ),
            new(
                Name: "Requester",
                Users: 3,
                Avatars: ["CF", "NI", "JD"],
                ExtraUsers: null,
                Badge: null,
                Permissions: permissions
                    .Select(p => p with
                    {
                        Edit = false,
                        Create = p.Menu is "RFx Management" or "Purchase Orders",
                        Delete = false
                    })
                    .ToList()
            )
        };

        var response = new ProcurementRoleResponse(
            MainRole: Roles.Procurement,
            SubRoles: subRoles,
            MenuPermissions: permissions
        );

        return Ok(ApiResponse<ProcurementRoleResponse>.Ok(response, "Procurement roles loaded."));
    }
}
