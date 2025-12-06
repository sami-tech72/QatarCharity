using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Models.Procurement;
using Domain.Entities.Procurement;
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

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProcurementSubRole>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ProcurementSubRole>>> CreateProcurementRole(
        [FromBody] CreateProcurementRoleRequest request)
    {
        var permissionDefinitions = await _dbContext.ProcurementPermissionDefinitions
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        var permissionsByName = (request.Permissions ?? Array.Empty<ProcurementPermission>())
            .ToDictionary(permission => permission.Name, permission => permission.Actions, StringComparer.OrdinalIgnoreCase);

        var template = new ProcurementRoleTemplate
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? "Custom procurement role"
                : request.Description.Trim(),
            TotalUsers = 0,
            NewUsers = 0,
            Permissions = MapRolePermissions(permissionDefinitions, permissionsByName),
        };

        _dbContext.ProcurementRoleTemplates.Add(template);
        await _dbContext.SaveChangesAsync();

        var subRole = new ProcurementSubRole(
            Name: template.Name,
            Description: template.Description,
            TotalUsers: template.TotalUsers,
            NewUsers: template.NewUsers,
            Avatars: template.Avatars.Select(a => a.FileName).ToList(),
            ExtraCount: template.ExtraCount,
            Permissions: MapPermissions(template.Permissions, permissionDefinitions));

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ProcurementSubRole>.Ok(subRole, "Procurement role created successfully."));
    }

    private static IReadOnlyList<ProcurementPermission> MapPermissions(
        IEnumerable<ProcurementRolePermission> rolePermissions,
        IEnumerable<ProcurementPermissionDefinition> definitions)
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

    private static List<ProcurementRolePermission> MapRolePermissions(
        IEnumerable<ProcurementPermissionDefinition> definitions,
        IReadOnlyDictionary<string, ProcurementPermissionActions> overrides)
    {
        return definitions
            .Select(definition =>
            {
                var hasOverride = overrides.TryGetValue(definition.Name, out var actions);
                var read = hasOverride ? actions!.Read : definition.DefaultRead;
                var write = hasOverride ? actions!.Write : definition.DefaultWrite;
                var create = hasOverride ? actions!.Create : definition.DefaultCreate;

                return new ProcurementRolePermission
                {
                    ProcurementPermissionDefinitionId = definition.Id,
                    CanRead = read,
                    CanWrite = write,
                    CanCreate = create,
                };
            })
            .ToList();
    }
}
