using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
[Route("api/rfx")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Procurement}")]
public class RfxController : ControllerBase
{
    private readonly GetSupplierBidsQuery _getSupplierBids;
    private readonly GetRfxListQuery _getRfxList;
    private readonly CreateRfxCommand _createRfx;
    private readonly GetRfxByIdQuery _getRfxById;
    private readonly EvaluateBidCommand _evaluateBid;
    private readonly ApproveRfxCommand _approveRfx;
    private readonly CloseRfxCommand _closeRfx;

    public RfxController(
        GetSupplierBidsQuery getSupplierBids,
        GetRfxListQuery getRfxList,
        CreateRfxCommand createRfx,
        GetRfxByIdQuery getRfxById,
        EvaluateBidCommand evaluateBid,
        ApproveRfxCommand approveRfx,
        CloseRfxCommand closeRfx)
    {
        _getSupplierBids = getSupplierBids;
        _getRfxList = getRfxList;
        _createRfx = createRfx;
        _getRfxById = getRfxById;
        _evaluateBid = evaluateBid;
        _approveRfx = approveRfx;
        _closeRfx = closeRfx;
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidResponse>>>> GetSupplierBids([FromQuery] SupplierBidQueryParameters query)
    {
        var result = await _getSupplierBids.HandleAsync(query);
        return Ok(ApiResponse<PagedResult<SupplierBidResponse>>.Ok(result.Value!, "Supplier bids retrieved successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RfxSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RfxSummaryResponse>>>> GetRfxList([FromQuery] RfxQueryParameters query)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _getRfxList.HandleAsync(currentUserId, query);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse<PagedResult<RfxSummaryResponse>>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<PagedResult<RfxSummaryResponse>>.Ok(result.Value!, "RFx records retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> CreateRfx(CreateRfxRequest request)
    {
        var result = await _createRfx.HandleAsync(request);

        if (!result.Success)
        {
            var status = result.ErrorCode == "rfx_invalid_committee" || result.ErrorCode == "rfx_duplicate_title"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            return StatusCode(status, ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<RfxDetailResponse>.Ok(result.Value!, "RFx created successfully."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> GetRfxById(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _getRfxById.HandleAsync(id, currentUserId);

        if (!result.Success)
        {
            return result.ErrorCode == "auth_invalid_token"
                ? Unauthorized(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode))
                : result.ErrorCode == "rfx_not_found"
                    ? NotFound(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode))
                    : BadRequest(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<RfxDetailResponse>.Ok(result.Value!, "RFx details retrieved successfully."));
    }

    [HttpPost("{rfxId:guid}/bids/{bidId:guid}/evaluate")]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierBidResponse>>> EvaluateBid(Guid rfxId, Guid bidId, [FromBody] EvaluateBidRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _evaluateBid.HandleAsync(rfxId, bidId, request, currentUserId);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "rfx_not_found" => NotFound(ApiResponse<SupplierBidResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                "bid_not_found" => NotFound(ApiResponse<SupplierBidResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                _ => BadRequest(ApiResponse<SupplierBidResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
            };
        }

        return Ok(ApiResponse<SupplierBidResponse>.Ok(result.Value!, "Bid evaluation saved."));
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> ApproveRfx(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _approveRfx.HandleAsync(id, currentUserId);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "auth_invalid_token" => Unauthorized(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                "rfx_not_found" => NotFound(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                "forbidden" => Forbid(),
                _ => BadRequest(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
            };
        }

        return Ok(ApiResponse<RfxDetailResponse>.Ok(result.Value!, "RFx approved and published."));
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> CloseRfx(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _closeRfx.HandleAsync(id, currentUserId, User.IsInRole(Roles.Admin));

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "auth_invalid_token" => Unauthorized(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                "rfx_not_found" => NotFound(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
                "forbidden" => Forbid(),
                _ => BadRequest(ApiResponse<RfxDetailResponse>.Fail(result.ErrorMessage!, result.ErrorCode)),
            };
        }

        return Ok(ApiResponse<RfxDetailResponse>.Ok(result.Value!, "RFx closed."));
    }

    private string GetCurrentUserId()
    {
        var principal = HttpContext?.User ?? User;
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            var token = principal?.FindFirstValue("id")
                ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            userId = token;
        }

        return userId ?? string.Empty;
    }
}
