using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/supplier/rfx")]
[Authorize(Roles = Roles.Supplier)]
public class SupplierRfxController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    private record SupplierBidProjection(
        SupplierBid bid,
        Domain.Entities.Rfx rfx,
        string SupplierName,
        string SubmittedByUserId,
        string SubmittedByName);

    public SupplierRfxController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidSummaryResponse>>>> GetMySupplierBids([FromQuery] BidQueryParameters query)
    {
        var bidderId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(bidderId))
        {
            return Unauthorized(ApiResponse<PagedResult<SupplierBidSummaryResponse>>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();
        var bidderName = User?.Identity?.Name ?? "You";

        var bidsQuery = BuildSupplierBidQuery(bidderId, bidderName, search);

        var totalCount = await bidsQuery.CountAsync();

        var pagedEntries = await bidsQuery
            .OrderByDescending(entry => entry.bid.SubmittedAtUtc)
            .ThenBy(entry => entry.rfx.ReferenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var reviewLookup = await BuildReviewLookupAsync(pagedEntries.Select(entry => entry.bid.Id));
        var bids = pagedEntries
            .Select(entry => BuildBidSummary(entry, reviewLookup))
            .ToList();

        var response = new PagedResult<SupplierBidSummaryResponse>(bids, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<SupplierBidSummaryResponse>>.Ok(response, "Supplier bids retrieved successfully."));
    }

    [HttpGet("published")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PublishedRfxResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PublishedRfxResponse>>>> GetPublishedRfx([FromQuery] SupplierRfxQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var rfxQuery = _dbContext.Rfxes
            .AsNoTracking()
            .Where(rfx => rfx.Status != null && rfx.Status.ToLower() == "published");

        if (!string.IsNullOrWhiteSpace(search))
        {
            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(search) ||
                (rfx.Title ?? string.Empty).ToLower().Contains(search) ||
                (rfx.Category ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await rfxQuery.CountAsync();

        var tenders = await rfxQuery
            .OrderBy(rfx => rfx.SubmissionDeadline)
            .ThenBy(rfx => rfx.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(rfx => BuildPublishedRfxResponse(rfx))
            .ToListAsync();

        var response = new PagedResult<PublishedRfxResponse>(tenders, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<PublishedRfxResponse>>.Ok(response, "Published RFx records retrieved successfully."));
    }

    [HttpPost("{rfxId:guid}/bid")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> SubmitBid(Guid rfxId, [FromBody] SubmitBidRequest? request)
    {
        if (request is null)
        {
            return BadRequest(ApiResponse<string>.Fail("Bid request payload is required.", "invalid_request"));
        }

        if (request.BidAmount <= 0)
        {
            return BadRequest(ApiResponse<string>.Fail("Bid amount must be greater than zero.", "invalid_bid_amount"));
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return BadRequest(ApiResponse<string>.Fail("Currency is required for bid submission.", "invalid_currency"));
        }

        if (request.ExpectedDeliveryDate is null)
        {
            return BadRequest(ApiResponse<string>.Fail("Expected delivery date is required.", "invalid_delivery_date"));
        }

        if (string.IsNullOrWhiteSpace(request.ProposalSummary))
        {
            return BadRequest(ApiResponse<string>.Fail("Proposal summary is required.", "invalid_proposal_summary"));
        }

        var rfx = await _dbContext.Rfxes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rfxId);

        if (rfx is null)
        {
            return NotFound(ApiResponse<string>.Fail("Tender not found.", "rfx_not_found"));
        }

        if (!string.Equals(rfx.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<string>.Fail("This tender is not open for bids.", "rfx_not_published"));
        }

        var requiredDocuments = DeserializeList(rfx.RequiredDocuments);
        if (requiredDocuments.Any())
        {
            if (request.Documents is null || !request.Documents.Any())
            {
                return BadRequest(ApiResponse<string>.Fail("All required documents must be provided.", "documents_missing"));
            }

            var missingDocs = requiredDocuments
                .Where(doc => !request.Documents!.Any(submission =>
                    string.Equals(submission.Name, doc, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(submission.FileName) &&
                    !string.IsNullOrWhiteSpace(submission.ContentBase64)))
                .ToList();

            if (missingDocs.Any())
            {
                return BadRequest(ApiResponse<string>.Fail(
                    $"Missing required document details: {string.Join(", ", missingDocs)}.",
                    "documents_incomplete"));
            }

            var invalidDocument = request.Documents!.FirstOrDefault(doc => !IsBase64(doc.ContentBase64));
            if (invalidDocument is not null)
            {
                return BadRequest(ApiResponse<string>.Fail(
                    $"The uploaded document for '{invalidDocument.Name}' is invalid or unreadable.",
                    "documents_invalid"));
            }
        }

        var requiredInputs = BuildRequiredInputs(rfx);
        if (requiredInputs.Any())
        {
            if (request.Inputs is null || !request.Inputs.Any())
            {
                return BadRequest(ApiResponse<string>.Fail("All required input data must be provided.", "inputs_missing"));
            }

            var missingInputs = requiredInputs
                .Where(input => !request.Inputs!.Any(submission =>
                    string.Equals(submission.Name, input, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(submission.Value)))
                .ToList();

            if (missingInputs.Any())
            {
                return BadRequest(ApiResponse<string>.Fail(
                    $"Missing required input data: {string.Join(", ", missingInputs)}.",
                    "inputs_incomplete"));
            }
        }

        var bidderId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name ?? "unknown";
        var bid = new SupplierBid
        {
            RfxId = rfx.Id,
            SubmittedByUserId = bidderId,
            BidAmount = request.BidAmount,
            Currency = request.Currency,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            ProposalSummary = request.ProposalSummary,
            Notes = request.Notes,
            DocumentsJson = JsonSerializer.Serialize(request.Documents ?? Array.Empty<BidDocumentSubmission>()),
            InputsJson = JsonSerializer.Serialize(request.Inputs ?? Array.Empty<BidInputSubmission>()),
            SubmittedAtUtc = DateTime.UtcNow,
        };

        _dbContext.SupplierBids.Add(bid);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Bid submitted successfully.", "Bid submitted successfully."));
    }

    [HttpGet("published/{rfxId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublishedRfxResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PublishedRfxResponse>>> GetPublishedRfxById(Guid rfxId)
    {
        var rfx = await _dbContext.Rfxes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rfxId);

        if (rfx is null)
        {
            return NotFound(ApiResponse<PublishedRfxResponse>.Fail("Tender not found.", "rfx_not_found"));
        }

        if (!string.Equals(rfx.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<PublishedRfxResponse>.Fail("This tender is not open for bids.", "rfx_not_published"));
        }

        var response = BuildPublishedRfxResponse(rfx);

        return Ok(ApiResponse<PublishedRfxResponse>.Ok(response, "Published RFx retrieved successfully."));
    }

    private IQueryable<SupplierBidProjection> BuildSupplierBidQuery(string bidderId, string bidderName, string? search)
    {
        var bidsQuery = _dbContext.SupplierBids
            .AsNoTracking()
            .Where(bid => bid.SubmittedByUserId == bidderId)
            .Join(
                _dbContext.Rfxes.AsNoTracking(),
                bid => bid.RfxId,
                rfx => rfx.Id,
                (bid, rfx) => new { bid, rfx })
            .GroupJoin(
                _dbContext.Users.AsNoTracking(),
                entry => entry.bid.SubmittedByUserId,
                user => user.Id,
                (entry, users) => new { entry.bid, entry.rfx, users })
            .SelectMany(
                entry => entry.users.DefaultIfEmpty(),
                (entry, user) => new SupplierBidProjection(
                    entry.bid,
                    entry.rfx,
                    user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "You"
                        : (string.IsNullOrWhiteSpace(bidderName) ? "You" : bidderName),
                    entry.bid.SubmittedByUserId,
                    user != null
                        ? user.DisplayName ?? user.Email ?? user.UserName ?? "You"
                        : (string.IsNullOrWhiteSpace(bidderName) ? "You" : bidderName)))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            bidsQuery = bidsQuery.Where(entry =>
                (entry.rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(search) ||
                (entry.rfx.Title ?? string.Empty).ToLower().Contains(search) ||
                entry.SupplierName.ToLower().Contains(search) ||
                entry.SubmittedByName.ToLower().Contains(search));
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

    private static SupplierBidSummaryResponse BuildBidSummary(SupplierBidProjection entry, IReadOnlyDictionary<Guid, List<BidReviewResponse>> reviewsLookup)
    {
        reviewsLookup.TryGetValue(entry.bid.Id, out var reviews);

        return new SupplierBidSummaryResponse(
            entry.bid.Id,
            entry.bid.RfxId,
            entry.rfx.ReferenceNumber ?? string.Empty,
            entry.rfx.Title ?? string.Empty,
            entry.SupplierName,
            entry.SubmittedByUserId,
            entry.SubmittedByName,
            entry.bid.BidAmount,
            entry.bid.Currency,
            entry.bid.ExpectedDeliveryDate,
            entry.bid.SubmittedAtUtc,
            entry.bid.ProposalSummary,
            entry.bid.Notes,
            reviews ?? new List<BidReviewResponse>());
    }

    private static bool IsBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            _ = Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> DeserializeList(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return new List<string>();
        }

        try
        {
            var values = System.Text.Json.JsonSerializer.Deserialize<List<string>>(serialized);
            return values ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static IReadOnlyCollection<string> BuildRequirementDetails(Domain.Entities.Rfx rfx)
    {
        var details = new List<string>();

        if (!string.IsNullOrWhiteSpace(rfx.Scope))
        {
            details.Add(rfx.Scope);
        }

        if (!string.IsNullOrWhiteSpace(rfx.TechnicalSpecification))
        {
            details.Add(rfx.TechnicalSpecification);
        }

        if (!string.IsNullOrWhiteSpace(rfx.Deliverables))
        {
            details.Add(rfx.Deliverables);
        }

        if (!string.IsNullOrWhiteSpace(rfx.Timeline))
        {
            details.Add(rfx.Timeline);
        }

        return details;
    }

    private static IReadOnlyCollection<string> BuildRequiredInputs(Domain.Entities.Rfx rfx)
    {
        var inputs = new List<string>
        {
            "Bid amount",
            "Expected delivery date",
            "Proposal summary"
        };

        if (!string.IsNullOrWhiteSpace(rfx.TechnicalSpecification))
        {
            inputs.Add("Technical compliance notes");
        }

        if (!string.IsNullOrWhiteSpace(rfx.Deliverables))
        {
            inputs.Add("Delivery approach for required deliverables");
        }

        return inputs;
    }

    private static PublishedRfxResponse BuildPublishedRfxResponse(Domain.Entities.Rfx rfx)
    {
        return new PublishedRfxResponse(
            rfx.Id,
            rfx.ReferenceNumber,
            rfx.RfxType,
            rfx.Title,
            rfx.Category,
            rfx.Description,
            rfx.PublicationDate,
            rfx.SubmissionDeadline,
            rfx.ClosingDate,
            rfx.EstimatedBudget,
            rfx.Currency,
            rfx.HideBudget,
            rfx.Scope,
            rfx.TechnicalSpecification,
            rfx.Deliverables,
            rfx.Timeline,
            DeserializeList(rfx.RequiredDocuments),
            BuildRequirementDetails(rfx),
            BuildRequiredInputs(rfx));
    }
}
