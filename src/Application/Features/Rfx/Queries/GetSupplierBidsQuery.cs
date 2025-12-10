using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
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

        var bidIds = results
            .Where(entry => entry.Bid != null)
            .Select(entry => entry.Bid!.Id)
            .ToList();

        var reviewsLookup = await _repository.GetBidReviewsAsync(bidIds);

        // collect submitted & evaluated user ids
        var userIds = results
            .Where(e => e.Bid != null)
            .Select(e => e.Bid!.SubmittedByUserId)
            .Concat(results.Select(e => e.Bid?.EvaluatedByUserId ?? string.Empty))
            .Concat(reviewsLookup.Values.SelectMany(r => r.Select(review => review.ReviewerUserId)))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var userLookup = await _userDirectoryService.GetUserNamesAsync(userIds);

        var bidResponses = results
            // list view: we don't need full reviews, so pass only basic info
            .Select(entry =>
            {
                var reviews = reviewsLookup.TryGetValue(entry.Bid!.Id, out var bidReviews)
                    ? (IReadOnlyCollection<SupplierBidReview>)bidReviews
                    : Array.Empty<SupplierBidReview>();

                return RfxMapping.BuildBidResponse(entry.Bid!, entry.Rfx!, userLookup, reviews);
            })
            .ToList();

        var pagedResult = new PagedResult<SupplierBidResponse>(bidResponses, totalCount, pageNumber, pageSize);

        return Result<PagedResult<SupplierBidResponse>>.Ok(pagedResult);
    }
}
