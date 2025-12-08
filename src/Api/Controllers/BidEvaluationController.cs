using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization;
using Api.Models;
using Api.Models.Rfx;
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
                (entry, user) => new
                {
                    entry.bid,
                    entry.rfx,
                    SupplierName = user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "Supplier"
                        : "Supplier",
                    SubmittedByUserId = entry.bid.SubmittedByUserId,
                    SubmittedByName = user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "Supplier User"
                        : "Supplier User",
                })
            .AsQueryable();

        if (isProcurementSubRole)
        {
            bidsQuery = bidsQuery
                .Join(
                    _dbContext.RfxCommitteeMembers.AsNoTracking(),
                    entry => entry.rfx.Id,
                    member => member.RfxId,
                    (entry, member) => new
                    {
                        entry.bid,
                        entry.rfx,
                        entry.SupplierName,
                        entry.SubmittedByUserId,
                        entry.SubmittedByName,
                        member.UserId,
                    })
                .Where(entry => entry.UserId == currentUserId)
                .Select(entry => new
                {
                    entry.bid,
                    entry.rfx,
                    entry.SupplierName,
                    entry.SubmittedByUserId,
                    entry.SubmittedByName,
                });
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            bidsQuery = bidsQuery.Where(entry =>
                entry.rfx.ReferenceNumber.ToLower().Contains(search) ||
                entry.rfx.Title.ToLower().Contains(search) ||
                entry.SupplierName.ToLower().Contains(search) ||
                entry.SubmittedByName.ToLower().Contains(search));
        }

        var totalCount = await bidsQuery.CountAsync();

        var bids = await bidsQuery
            .OrderByDescending(entry => entry.bid.SubmittedAtUtc)
            .ThenBy(entry => entry.rfx.ReferenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(entry => new SupplierBidSummaryResponse(
                entry.bid.Id,
                entry.bid.RfxId,
                entry.rfx.ReferenceNumber,
                entry.rfx.Title,
                entry.SupplierName,
                entry.SubmittedByUserId,
                entry.SubmittedByName,
                entry.bid.BidAmount,
                entry.bid.Currency,
                entry.bid.ExpectedDeliveryDate,
                entry.bid.SubmittedAtUtc,
                entry.bid.ProposalSummary,
                entry.bid.Notes))
            .ToListAsync();

        var response = new PagedResult<SupplierBidSummaryResponse>(bids, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<SupplierBidSummaryResponse>>.Ok(response, "Supplier bids retrieved successfully."));
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
