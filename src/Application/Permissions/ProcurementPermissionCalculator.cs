using System.Collections.Generic;
using Application.DTOs;
using Domain.Enums;

namespace Application.Permissions;

public static class ProcurementPermissionCalculator
{
    private static readonly IReadOnlyDictionary<string, ProcurementPermissionSet> _map =
        new Dictionary<string, ProcurementPermissionSet>(System.StringComparer.OrdinalIgnoreCase)
        {
            [ProcurementSubRoles.Viewer] = new(CanView: true),
            [ProcurementSubRoles.Contributor] = new(CanView: true, CanCreate: true, CanUpdate: true),
            [ProcurementSubRoles.Manager] = new(CanView: true, CanCreate: true, CanUpdate: true, CanDelete: true),
        };

    public static ProcurementPermissionSet CombineFor(IEnumerable<string> subRoles)
    {
        var combined = new ProcurementPermissionSet();

        foreach (var subRole in subRoles)
        {
            if (!_map.TryGetValue(subRole, out var permissions))
            {
                continue;
            }

            combined = combined.Merge(permissions);
        }

        return combined;
    }
}
