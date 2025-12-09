using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;

namespace Application.Features.Rfx.Queries;

public class GetSupplierBidsQuery(IRfxRepository repository, IUserDirectoryService userDirectoryService)
{
    public async Task<Result<PagedResult<SupplierBidResponse>>> HandleAsync(SupplierBidQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountSupplierBidsAsync(search);
        var results = await repository.GetSupplierBidsAsync(search, pageNumber, pageSize);

        var userLookup = await userDirectoryService.GetUserNamesAsync(results
            .Select(entry => entry.Bid.SubmittedByUserId)
            .Concat(results.Select(entry => entry.Bid.EvaluatedByUserId ?? string.Empty))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct());

        var bidResponses = results
            .Select(entry => RfxMapping.BuildBidResponse(entry.Bid, entry.Rfx, userLookup))
            .ToList();

        var pagedResult = new PagedResult<SupplierBidResponse>(bidResponses, totalCount, pageNumber, pageSize);

        return Result<PagedResult<SupplierBidResponse>>.Ok(pagedResult);
    }
}
