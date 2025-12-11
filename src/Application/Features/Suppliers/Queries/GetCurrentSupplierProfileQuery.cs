using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Suppliers.Queries;

public class GetCurrentSupplierProfileQuery(ISupplierRepository repository)
{
    public async Task<Result<SupplierResponse>> HandleAsync(string portalUserId)
    {
        var supplier = await repository.GetByPortalUserIdAsync(portalUserId);
        if (supplier is null)
        {
            return Result<SupplierResponse>.Fail("suppliers_not_found", "Supplier not found.");
        }

        return Result<SupplierResponse>.Ok(SupplierMapping.ToResponse(supplier));
    }
}
