namespace Application.DTOs;

public record ProcurementPermissionSet(
    bool CanView = false,
    bool CanCreate = false,
    bool CanUpdate = false,
    bool CanDelete = false)
{
    public ProcurementPermissionSet Merge(ProcurementPermissionSet other) => new(
        CanView || other.CanView,
        CanCreate || other.CanCreate,
        CanUpdate || other.CanUpdate,
        CanDelete || other.CanDelete);
}
