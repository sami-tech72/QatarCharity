using System.Collections.Generic;
using System.Linq;

namespace Domain.Authorization;

public static class ProcurementSubRoles
{
    public const string Viewer = "Viewer";
    public const string Creator = "Creator";
    public const string Editor = "Editor";
    public const string Manager = "Manager";

    public static readonly string[] All = [Viewer, Creator, Editor, Manager];

    private static readonly Dictionary<string, string[]> PermissionMap = new()
    {
        [Viewer] = [ProcurementPermissions.View],
        [Creator] = [ProcurementPermissions.View, ProcurementPermissions.Create],
        [Editor] = [ProcurementPermissions.View, ProcurementPermissions.Update],
        [Manager] = ProcurementPermissions.All,
    };

    public static string[] PermissionsFor(IEnumerable<string> subRoles)
    {
        var permissions = new HashSet<string>();

        foreach (var subRole in subRoles)
        {
            if (PermissionMap.TryGetValue(subRole, out var mappedPermissions))
            {
                permissions.UnionWith(mappedPermissions);
            }
        }

        return permissions.ToArray();
    }
}
