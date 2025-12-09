using Application.DTOs.Common;
using Application.DTOs.Suppliers;
using Application.Models;

namespace Application.Interfaces.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierResponse>> GetSuppliersAsync(SupplierQueryParameters query);

    Task<Result<SupplierResponse>> CreateSupplierAsync(UpsertSupplierRequest request);

    Task<Result<SupplierResponse>> UpdateSupplierAsync(Guid id, UpsertSupplierRequest request);
}
