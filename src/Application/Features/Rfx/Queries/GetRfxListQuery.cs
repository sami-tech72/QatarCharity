using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Models;
using Application.Interfaces.Repositories;

namespace Application.Features.Rfx.Queries;

public class GetRfxListQuery(IRfxRepository repository)
{
    public async Task<Result<PagedResult<RfxSummaryResponse>>> HandleAsync(string currentUserId, RfxQueryParameters query)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Result<PagedResult<RfxSummaryResponse>>.Fail("auth_invalid_token", "Invalid or expired token.");
        }

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountRfxAsync(search, query.AssignedOnly, currentUserId);
        var rfxes = await repository.GetRfxSummariesAsync(search, query.AssignedOnly, currentUserId, pageNumber, pageSize);

        var summaries = rfxes
            .Select(rfx => new
            {
                Entity = rfx,
                CommitteeCount = rfx.CommitteeMembers.Count,
                ApprovedCount = rfx.CommitteeMembers.Count(member => member.IsApproved),
                CanApprove = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId),
            })
            .Select(entry => new RfxSummaryResponse(
                entry.Entity.Id,
                entry.Entity.ReferenceNumber,
                entry.Entity.Title,
                entry.Entity.Category,
                entry.Entity.Status,
                entry.Entity.Status.Equals("Published", StringComparison.OrdinalIgnoreCase)
                    ? "Approved"
                    : entry.CommitteeCount > 0
                        ? $"{entry.ApprovedCount}/{entry.CommitteeCount} Approved"
                        : "Pending",
                entry.Entity.ClosingDate,
                entry.Entity.EstimatedBudget,
                entry.Entity.Currency,
                entry.Entity.Workflow?.Name,
                entry.CanApprove))
            .ToList();

        var pagedResult = new PagedResult<RfxSummaryResponse>(summaries, totalCount, pageNumber, pageSize);
        return Result<PagedResult<RfxSummaryResponse>>.Ok(pagedResult);
    }
}
