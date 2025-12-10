using System;
using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Commands;
using Application.Features.Rfx.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/supplier/rfx")]
[Authorize(Roles = Roles.Supplier)]
public class SupplierRfxController : ControllerBase
{
    private readonly GetPublishedRfxListQuery _getPublishedRfxListQuery;
    private readonly GetPublishedRfxByIdQuery _getPublishedRfxByIdQuery;
    private readonly SubmitBidCommand _submitBidCommand;
    private readonly GetSupplierBidsQuery _getSupplierBidsQuery;

    public SupplierRfxController(
        GetPublishedRfxListQuery getPublishedRfxListQuery,
        GetPublishedRfxByIdQuery getPublishedRfxByIdQuery,
        SubmitBidCommand submitBidCommand,
        GetSupplierBidsQuery getSupplierBidsQuery)
    {
        _getPublishedRfxListQuery = getPublishedRfxListQuery;
        _getPublishedRfxByIdQuery = getPublishedRfxByIdQuery;
        _submitBidCommand = submitBidCommand;
        _getSupplierBidsQuery = getSupplierBidsQuery;
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidResponse>>>> GetMyBids([FromQuery] SupplierBidQueryParameters query)
    {
        var currentUserId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name ?? string.Empty;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<PagedResult<SupplierBidResponse>>.Fail("Unauthorized", "auth_invalid_token"));
        }

        query.SubmittedByUserId = currentUserId;

        var result = await _getSupplierBidsQuery.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<SupplierBidResponse>>.Ok(result.Value!, "Supplier bids retrieved successfully."));
    }

    [HttpGet("published")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PublishedRfxResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PublishedRfxResponse>>>> GetPublishedRfx([FromQuery] SupplierRfxQueryParameters query)
    {
        var result = await _getPublishedRfxListQuery.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<PublishedRfxResponse>>.Ok(result.Value!, "Published RFx records retrieved successfully."));
    }

    [HttpPost("{rfxId:guid}/bid")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> SubmitBid(Guid rfxId, [FromBody] SubmitBidRequest? request)
    {
        var bidderId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name ?? "unknown";
        var result = await _submitBidCommand.HandleAsync(rfxId, bidderId, request ?? new SubmitBidRequest());

        if (!result.IsSuccess)
        {
            var status = result.ErrorCode switch
            {
                "rfx_not_found" => StatusCodes.Status404NotFound,
                "rfx_not_published" => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest,
            };

            return StatusCode(status, ApiResponse<string>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<string>.Ok(result.Value!, "Bid submitted successfully."));
    }

    [HttpGet("published/{rfxId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublishedRfxResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PublishedRfxResponse>>> GetPublishedRfxById(Guid rfxId)
    {
        var result = await _getPublishedRfxByIdQuery.HandleAsync(rfxId);

        if (!result.IsSuccess)
        {
            var status = result.ErrorCode == "rfx_not_found"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(status, ApiResponse<PublishedRfxResponse>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<PublishedRfxResponse>.Ok(result.Value!, "Published RFx retrieved successfully."));
    }
}
