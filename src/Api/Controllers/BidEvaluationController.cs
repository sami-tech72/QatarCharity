using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization;
using Api.Models;
using Api.Models.Rfx;
using Domain.Entities;
using Domain.Entities.Procurement;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Controllers;

[ApiController]
[Route("api/rfx/bids")]
[Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
public class BidEvaluationController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    private record BidProjection(
        SupplierBid Bid,
        Domain.Entities.Rfx Rfx,
        string SupplierName,
        string SubmittedByUserId,
        string SubmittedByName);

    public BidEvaluationController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Policy = ProcurementPolicies.BidEvaluationRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidSummaryResponse>>>> GetSupplierBids([FromQuery] BidQueryParameters query)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<PagedResult<SupplierBidSummaryResponse>>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();
        var isProcurementSubRole = User.HasClaim(claim => string.Equals(claim.Type, "procurement_role_id", StringComparison.OrdinalIgnoreCase));

        var bidsQuery = BuildBidQuery(currentUserId, isProcurementSubRole, search);

        var totalCount = await bidsQuery.CountAsync();

        var pagedEntries = await bidsQuery
            .OrderByDescending(entry => entry.Bid.SubmittedAtUtc)
            .ThenBy(entry => entry.Rfx.ReferenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var reviewLookup = await BuildReviewLookupAsync(pagedEntries.Select(entry => entry.Bid.Id));
        var bids = pagedEntries
            .Select(entry => BuildBidResponse(entry, reviewLookup))
            .ToList();

        var response = new PagedResult<SupplierBidSummaryResponse>(bids, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<SupplierBidSummaryResponse>>.Ok(response, "Supplier bids retrieved successfully."));
    }

    [HttpPost("{bidId:guid}/review")]
    [Authorize(Policy = ProcurementPolicies.BidEvaluationRead)]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupplierBidSummaryResponse>>> ReviewBid(
        Guid bidId,
        [FromBody] ReviewBidRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest(ApiResponse<SupplierBidSummaryResponse>.Fail(
                "A decision is required to review the bid.",
                "invalid_decision"));
        }

        var normalizedDecision = NormalizeDecision(request.Decision);
        if (string.IsNullOrWhiteSpace(normalizedDecision))
        {
            return BadRequest(ApiResponse<SupplierBidSummaryResponse>.Fail(
                "Decision must be approve or reject.",
                "invalid_decision"));
        }

        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<SupplierBidSummaryResponse>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var isProcurementSubRole = User.HasClaim(claim => string.Equals(claim.Type, "procurement_role_id", StringComparison.OrdinalIgnoreCase));
        var bidQuery = BuildBidQuery(currentUserId, isProcurementSubRole, search: null);
        var targetBid = await bidQuery.FirstOrDefaultAsync(entry => entry.Bid.Id == bidId);

        if (targetBid is null)
        {
            return NotFound(ApiResponse<SupplierBidSummaryResponse>.Fail(
                "Bid not found or not accessible.",
                "bid_not_found"));
        }

        var existing = await _dbContext.BidReviews
            .FirstOrDefaultAsync(review => review.BidId == bidId && review.ReviewerUserId == currentUserId);

        if (existing is null)
        {
            _dbContext.BidReviews.Add(new BidReview
            {
                BidId = bidId,
                ReviewerUserId = currentUserId,
                Decision = normalizedDecision,
                Comments = request.Comments?.Trim(),
                ReviewedAtUtc = DateTime.UtcNow,
            });
        }
        else
        {
            existing.Decision = normalizedDecision;
            existing.Comments = request.Comments?.Trim();
            existing.ReviewedAtUtc = DateTime.UtcNow;
            _dbContext.BidReviews.Update(existing);
        }

        await _dbContext.SaveChangesAsync();

        var reviewsLookup = await BuildReviewLookupAsync(new[] { bidId });
        var response = BuildBidResponse(targetBid, reviewsLookup);

        return Ok(ApiResponse<SupplierBidSummaryResponse>.Ok(response, "Bid review recorded."));
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    private IQueryable<BidProjection> BuildBidQuery(string currentUserId, bool isProcurementSubRole, string? search)
    {
        var bidsQuery = _dbContext.SupplierBids
            .AsNoTracking()
            .Join(
                _dbContext.Rfxes
                    .Include(rfx => rfx.CommitteeMembers)
                    .AsNoTracking(),
                bid => bid.RfxId,
                rfx => rfx.Id,
                (bid, rfx) => new { bid, rfx })
            .GroupJoin(
                _dbContext.Users.AsNoTracking(),
                combined => combined.bid.SubmittedByUserId,
                user => user.Id,
                (combined, users) => new { combined.bid, combined.rfx, users })
            .SelectMany(
                entry => entry.users.DefaultIfEmpty(),
                (entry, user) => new BidProjection(
                    entry.bid,
                    entry.rfx,
                    user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "Supplier"
                        : "Supplier",
                    entry.bid.SubmittedByUserId,
                    user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "Supplier User"
                        : "Supplier User"))
            .AsQueryable();

        if (isProcurementSubRole)
        {
            bidsQuery = bidsQuery
                .GroupJoin(
                    _dbContext.RfxCommitteeMembers.AsNoTracking(),
                    entry => entry.Rfx.Id,
                    member => member.RfxId,
                    (entry, members) => new { entry, members })
                .SelectMany(
                    x => x.members.DefaultIfEmpty(),
                    (x, member) => new { x.entry, member })
                .Where(entry => entry.member == null || entry.member.UserId == currentUserId)
                .Select(entry => entry.entry);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLowerInvariant();
            bidsQuery = bidsQuery.Where(entry =>
                (entry.Rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (entry.Rfx.Title ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                entry.SupplierName.ToLower().Contains(normalizedSearch) ||
                entry.SubmittedByName.ToLower().Contains(normalizedSearch));
        }

        return bidsQuery;
    }

    private async Task<Dictionary<Guid, List<BidReviewResponse>>> BuildReviewLookupAsync(IEnumerable<Guid> bidIds)
    {
        var targetIds = bidIds.ToList();

        if (!targetIds.Any())
        {
            return new();
        }

        var reviews = await _dbContext.BidReviews
            .AsNoTracking()
            .Where(review => targetIds.Contains(review.BidId))
            .GroupJoin(
                _dbContext.Users.AsNoTracking(),
                review => review.ReviewerUserId,
                user => user.Id,
                (review, users) => new { review, users })
            .SelectMany(
                entry => entry.users.DefaultIfEmpty(),
                (entry, user) => new BidReviewResponse(
                    entry.review.Id,
                    entry.review.BidId,
                    entry.review.ReviewerUserId,
                    user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? entry.review.ReviewerUserId
                        : entry.review.ReviewerUserId,
                    entry.review.Decision,
                    entry.review.ReviewedAtUtc,
                    entry.review.Comments))
            .ToListAsync();

        return reviews
            .GroupBy(review => review.BidId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(review => review.ReviewedAtUtc).ToList());
    }

    private SupplierBidSummaryResponse BuildBidResponse(
        BidProjection entry,
        IReadOnlyDictionary<Guid, List<BidReviewResponse>> reviewLookup)
    {
        reviewLookup.TryGetValue(entry.Bid.Id, out var reviews);

        return new SupplierBidSummaryResponse(
            entry.Bid.Id,
            entry.Bid.RfxId,
            entry.Rfx.ReferenceNumber,
            entry.Rfx.Title,
            entry.SupplierName,
            entry.SubmittedByUserId,
            entry.SubmittedByName,
            entry.Bid.BidAmount,
            entry.Bid.Currency,
            entry.Bid.ExpectedDeliveryDate,
            entry.Bid.SubmittedAtUtc,
            entry.Bid.ProposalSummary,
            entry.Bid.Notes,
            reviews ?? new List<BidReviewResponse>());
    }

    private string? NormalizeDecision(string decision)
    {
        var normalized = decision.Trim().ToLowerInvariant();

        return normalized switch
        {
            "approve" or "approved" or "accept" or "accepted" or "good" => "approved",
            "reject" or "rejected" or "decline" or "not good" or "bad" => "rejected",
            "review" or "under review" or "view" => "review",
            _ => null,
        };
    }
}
