using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
[Route("api/supplier/rfx")]
[Authorize(Roles = Roles.Supplier)]
public class SupplierRfxController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public SupplierRfxController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
            .Select(rfx => new PublishedRfxResponse(
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
                DeserializeList(rfx.RequiredDocuments)))
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

        // Persisting the bid is out of scope for this iteration; this endpoint validates payloads
        // and confirms that the tender is open for supplier bids.
        return Ok(ApiResponse<string>.Ok("Bid submitted successfully.", "Bid submitted successfully."));
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
}
