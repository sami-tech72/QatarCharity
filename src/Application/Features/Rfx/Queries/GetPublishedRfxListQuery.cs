using System.Linq;
using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Rfx.Queries;

public class GetPublishedRfxListQuery(IRfxRepository repository)
{
    public async Task<Result<PagedResult<PublishedRfxResponse>>> HandleAsync(SupplierRfxQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountPublishedRfxAsync(search);
        var rfxes = await repository.GetPublishedRfxAsync(search, pageNumber, pageSize);

        var responses = rfxes
            .OrderBy(rfx => rfx.SubmissionDeadline)
            .ThenBy(rfx => rfx.Title)
            .Select(RfxMapping.BuildPublishedRfxResponse)
            .ToList();

        var pagedResult = new PagedResult<PublishedRfxResponse>(responses, totalCount, pageNumber, pageSize);

        return Result<PagedResult<PublishedRfxResponse>>.Ok(pagedResult);
    }
}
