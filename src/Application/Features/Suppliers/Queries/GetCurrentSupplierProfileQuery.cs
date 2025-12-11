using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Suppliers.Queries;

public class GetCurrentSupplierProfileQuery(ISupplierRepository repository)
{
    public async Task<Result<SupplierResponse>> HandleAsync(string portalUserId, string? email, string? displayName)
    {
        var supplier = await repository.GetByPortalUserIdAsync(portalUserId)
                       ?? await EnsureSupplierShellAsync(portalUserId, email, displayName);

        return supplier is null
            ? Result<SupplierResponse>.Fail("suppliers_not_found", "Supplier not found.")
            : Result<SupplierResponse>.Ok(SupplierMapping.ToResponse(supplier));
    }

    private async Task<Supplier?> EnsureSupplierShellAsync(string portalUserId, string? email, string? displayName)
    {
        if (string.IsNullOrWhiteSpace(portalUserId))
        {
            return null;
        }

        var supplier = SupplierProfileFactory.CreateFromCurrentUser(portalUserId, email, displayName);
        await repository.AddAsync(supplier);
        await repository.SaveChangesAsync();

        return supplier;
    }
}
