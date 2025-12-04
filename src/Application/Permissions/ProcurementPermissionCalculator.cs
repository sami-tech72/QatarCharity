using System.Collections.Generic;
using System.Linq;
using Application.DTOs;
using Domain.Enums;

namespace Application.Permissions;

public static class ProcurementPermissionCalculator
{
    private static readonly IReadOnlyDictionary<string, ProcurementPermissionSet> _defaults =
        new Dictionary<string, ProcurementPermissionSet>(System.StringComparer.OrdinalIgnoreCase)
        {
            [ProcurementSubRoles.Viewer] = new(CanView: true),
            [ProcurementSubRoles.Contributor] = new(CanView: true, CanCreate: true, CanUpdate: true),
            [ProcurementSubRoles.Manager] = new(CanView: true, CanCreate: true, CanUpdate: true, CanDelete: true),
        };

    public static IReadOnlyCollection<ProcurementSubRoleGrant> ParseClaims(IEnumerable<string> subRoleClaims)
    {
        var grants = new List<ProcurementSubRoleGrant>();

        foreach (var claimValue in subRoleClaims)
        {
            if (string.IsNullOrWhiteSpace(claimValue))
            {
                continue;
            }

            var parts = claimValue.Split('|', 2, System.StringSplitOptions.TrimEntries);
            var name = parts[0];
            var permissionSet = parts.Length > 1
                ? ParsePermissions(parts[1])
                : _defaults.TryGetValue(name, out var defaultPermissions)
                    ? defaultPermissions
                    : new ProcurementPermissionSet();

            grants.Add(new ProcurementSubRoleGrant(name, permissionSet));
        }

        return grants;
    }

    public static ProcurementPermissionSet CombineFor(IEnumerable<ProcurementSubRoleGrant> grants)
    {
        var combined = new ProcurementPermissionSet();

        foreach (var grant in grants)
        {
            combined = combined.Merge(grant.Permissions);
        }

        return combined;
    }

    /// <summary>
    /// Formats a procurement sub-role and its permissions into the claim string consumed by <see cref="ParseClaims"/>.
    /// </summary>
    /// <param name="name">The display/name of the sub-role (e.g. "ProcurementReviewer").</param>
    /// <param name="permissions">The permissions granted to this sub-role.</param>
    /// <returns>A string in the format "RoleName|view,create,update,delete" (permissions omitted if none).</returns>
    public static string ToClaimValue(string name, ProcurementPermissionSet permissions)
    {
        var parts = new List<string>();

        if (permissions.CanView)
        {
            parts.Add("view");
        }

        if (permissions.CanCreate)
        {
            parts.Add("create");
        }

        if (permissions.CanUpdate)
        {
            parts.Add("update");
        }

        if (permissions.CanDelete)
        {
            parts.Add("delete");
        }

        if (parts.Count == 0)
        {
            return name;
        }

        return $"{name}|{string.Join(',', parts)}";
    }

    private static ProcurementPermissionSet ParsePermissions(string permissionSegment)
    {
        var permissions = permissionSegment
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(permission => permission.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new ProcurementPermissionSet(
            CanView: permissions.Contains("view"),
            CanCreate: permissions.Contains("create"),
            CanUpdate: permissions.Contains("update"),
            CanDelete: permissions.Contains("delete"));
    }
}
