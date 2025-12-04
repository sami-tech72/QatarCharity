using System.Collections.Generic;
using Application.DTOs;
using Domain.Enums;

namespace Application.Permissions;

public static class ProcurementPermissionCalculator
{
    private static readonly IReadOnlyDictionary<string, ProcurementPermissionSet> _map =
        new Dictionary<string, ProcurementPermissionSet>(System.StringComparer.OrdinalIgnoreCase)
        {
            [ProcurementSubRoles.Viewer] = new(canView: true),
            [ProcurementSubRoles.Contributor] = new(canView: true, canCreate: true, canUpdate: true),
            [ProcurementSubRoles.Manager] = new(canView: true, canCreate: true, canUpdate: true, canDelete: true),
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
