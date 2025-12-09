using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Interfaces.Services;
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
    private readonly IRfxService _rfxService;

    public RfxController(IRfxService rfxService)
    {
        _rfxService = rfxService;
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidResponse>>>> GetSupplierBids([FromQuery] SupplierBidQueryParameters query)
    {
        var result = await _rfxService.GetSupplierBidsAsync(query);
        return Ok(ApiResponse<PagedResult<SupplierBidResponse>>.Ok(result.Value!, "Supplier bids retrieved successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RfxSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RfxSummaryResponse>>>> GetRfxList([FromQuery] RfxQueryParameters query)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _rfxService.GetRfxListAsync(currentUserId, query);

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
        var result = await _rfxService.CreateRfxAsync(request);

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
        var result = await _rfxService.GetRfxByIdAsync(id, currentUserId);

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
        var result = await _rfxService.EvaluateBidAsync(rfxId, bidId, request, currentUserId);

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
        var result = await _rfxService.ApproveRfxAsync(id, currentUserId);

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
        var result = await _rfxService.CloseRfxAsync(id, currentUserId, User.IsInRole(Roles.Admin));

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
