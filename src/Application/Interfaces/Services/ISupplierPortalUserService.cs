using Application.DTOs.Authentication;

namespace Application.Interfaces.Services;

public interface ISupplierPortalUserService
{
    Task<PortalUserResult> EnsurePortalUserAsync(
        bool hasPortalAccess,
        string? portalEmail,
        string contactName,
        Guid? currentSupplierId);
}
