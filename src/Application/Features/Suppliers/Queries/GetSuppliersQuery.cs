using Application.DTOs.Common;
using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Suppliers.Queries;

public class GetSuppliersQuery(ISupplierRepository repository)
{
    public async Task<PagedResult<SupplierResponse>> HandleAsync(SupplierQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountAsync(search);
        var suppliers = await repository.GetPagedAsync(search, pageNumber, pageSize);

        var response = suppliers.Select(SupplierMapping.ToResponse).ToList();
        return new PagedResult<SupplierResponse>(response, totalCount, pageNumber, pageSize);
    }
}
