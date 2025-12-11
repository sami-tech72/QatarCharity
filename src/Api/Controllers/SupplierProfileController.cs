using System.Security.Claims;
using Api.Models;
using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Commands;
using Application.Features.Suppliers.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/supplier/profile")]
[Authorize(Roles = Roles.Supplier)]
public class SupplierProfileController : ControllerBase
{
    private readonly GetCurrentSupplierProfileQuery _getSupplierProfileQuery;
    private readonly UpdateCurrentSupplierProfileCommand _updateSupplierProfileCommand;

    public SupplierProfileController(
        GetCurrentSupplierProfileQuery getSupplierProfileQuery,
        UpdateCurrentSupplierProfileCommand updateSupplierProfileCommand)
    {
        _getSupplierProfileQuery = getSupplierProfileQuery;
        _updateSupplierProfileCommand = updateSupplierProfileCommand;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> GetProfile()
    {
        var currentUserId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<SupplierResponse>.Fail("Unauthorized", "auth_invalid_token"));
        }

        var currentUserEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
        var currentUserName = User?.FindFirst("name")?.Value ?? User?.Identity?.Name;

        var result = await _getSupplierProfileQuery.HandleAsync(currentUserId, currentUserEmail, currentUserName);
        if (!result.Success)
        {
            return NotFound(ApiResponse<SupplierResponse>.Fail(result.ErrorMessage ?? "Supplier not found.", result.ErrorCode));
        }

        return Ok(ApiResponse<SupplierResponse>.Ok(result.Value!, "Supplier profile retrieved successfully."));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> UpdateProfile([FromBody] SupplierProfileRequest request)
    {
        var currentUserId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<SupplierResponse>.Fail("Unauthorized", "auth_invalid_token"));
        }

        var result = await _updateSupplierProfileCommand.HandleAsync(currentUserId, request);
        if (!result.Success)
        {
            return NotFound(ApiResponse<SupplierResponse>.Fail(result.ErrorMessage ?? "Unable to update supplier profile.", result.ErrorCode));
        }

        return Ok(ApiResponse<SupplierResponse>.Ok(result.Value!, "Supplier profile updated successfully."));
    }

    private string ResolveUserId()
    {
        return User?.FindFirst("sub")?.Value
               ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User?.Identity?.Name
               ?? string.Empty;
    }
}
