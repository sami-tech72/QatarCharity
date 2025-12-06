using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Models.Procurement;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Controllers;

[ApiController]
[Route("api/procurement/roles")]
[Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
public class ProcurementRolesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProcurementRolesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ProcurementRolesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProcurementRolesResponse>>> GetProcurementRoles()
    {
        var permissionDefinitions = await _dbContext.ProcurementPermissionDefinitions
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        var roleTemplates = await _dbContext.ProcurementRoleTemplates
            .Include(r => r.Avatars)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.ProcurementPermissionDefinition)
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .ToListAsync();

        var defaultPermissions = permissionDefinitions
            .Select(definition => new ProcurementPermission(
                definition.Name,
                new ProcurementPermissionActions(
                    definition.DefaultRead,
                    definition.DefaultWrite,
                    definition.DefaultCreate)))
            .ToList();

        var subRoles = roleTemplates
            .Select(template => new ProcurementSubRole(
                Name: template.Name,
                Description: template.Description,
                TotalUsers: template.TotalUsers,
                NewUsers: template.NewUsers,
                Avatars: template.Avatars.Select(a => a.FileName).ToList(),
                ExtraCount: template.ExtraCount,
                Permissions: MapPermissions(template.Permissions, permissionDefinitions)))
            .ToList();

        var response = new ProcurementRolesResponse(
            MainRole: Roles.Procurement,
            SubRoles: subRoles,
            DefaultPermissions: defaultPermissions);

        return Ok(ApiResponse<ProcurementRolesResponse>.Ok(response, "Procurement roles retrieved successfully."));
    }

    private static IReadOnlyList<ProcurementPermission> MapPermissions(
        IEnumerable<Domain.Entities.Procurement.ProcurementRolePermission> rolePermissions,
        IEnumerable<Domain.Entities.Procurement.ProcurementPermissionDefinition> definitions)
    {
        var permissionLookup = rolePermissions.ToDictionary(p => p.ProcurementPermissionDefinitionId);

        return definitions
            .Select(definition =>
            {
                var matchingPermission = permissionLookup.GetValueOrDefault(definition.Id);

                return new ProcurementPermission(
                    definition.Name,
                    new ProcurementPermissionActions(
                        matchingPermission?.CanRead ?? definition.DefaultRead,
                        matchingPermission?.CanWrite ?? definition.DefaultWrite,
                        matchingPermission?.CanCreate ?? definition.DefaultCreate));
            })
            .ToList();
    }
}
