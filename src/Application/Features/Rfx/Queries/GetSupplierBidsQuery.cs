using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Rfx.Queries;

public class GetSupplierBidsQuery
{
    private readonly IRfxRepository _repository;
    private readonly IUserDirectoryService _userDirectoryService;

    public GetSupplierBidsQuery(
        IRfxRepository repository,
        IUserDirectoryService userDirectoryService)
    {
        _repository = repository;
        _userDirectoryService = userDirectoryService;
    }

    public async Task<Result<PagedResult<SupplierBidResponse>>> HandleAsync(SupplierBidQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await _repository.CountSupplierBidsAsync(search);
        var results = await _repository.GetSupplierBidsAsync(search, pageNumber, pageSize);

        // collect submitted & evaluated user ids
        var userIds = results
            .Where(e => e.Bid != null)
            .Select(e => e.Bid!.SubmittedByUserId)
            .Concat(results.Select(e => e.Bid?.EvaluatedByUserId ?? string.Empty))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var userLookup = await _userDirectoryService.GetUserNamesAsync(userIds);

        var bidResponses = results
            // list view: we don't need full reviews, so pass only basic info
            .Select(entry => RfxMapping.BuildBidResponse(entry.Bid!, entry.Rfx!, userLookup))
            .ToList();

        var pagedResult = new PagedResult<SupplierBidResponse>(bidResponses, totalCount, pageNumber, pageSize);

        return Result<PagedResult<SupplierBidResponse>>.Ok(pagedResult);
    }
}
