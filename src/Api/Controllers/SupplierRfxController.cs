using System;
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
                rfx.HideBudget))
            .ToListAsync();

        var response = new PagedResult<PublishedRfxResponse>(tenders, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<PublishedRfxResponse>>.Ok(response, "Published RFx records retrieved successfully."));
    }
}
