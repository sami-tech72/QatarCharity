using System;
using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Commands;
using Application.Features.Suppliers.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize(Roles = Roles.Admin)]
public class SuppliersController : ControllerBase
{
    private readonly GetSuppliersQuery _getSuppliers;
    private readonly CreateSupplierCommand _createSupplier;
    private readonly UpdateSupplierCommand _updateSupplier;

    public SuppliersController(
        GetSuppliersQuery getSuppliers,
        CreateSupplierCommand createSupplier,
        UpdateSupplierCommand updateSupplier)
    {
        _getSuppliers = getSuppliers;
        _createSupplier = createSupplier;
        _updateSupplier = updateSupplier;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierResponse>>>> GetSuppliers([FromQuery] SupplierQueryParameters query)
    {
        var pagedResult = await _getSuppliers.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<SupplierResponse>>.Ok(pagedResult, "Suppliers retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> CreateSupplier(UpsertSupplierRequest request)
    {
        var result = await _createSupplier.HandleAsync(request);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SupplierResponse>.Fail(result.ErrorMessage!, result.ErrorCode));
        }

        return Ok(ApiResponse<SupplierResponse>.Ok(result.Value!, "Supplier created successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> UpdateSupplier(Guid id, UpsertSupplierRequest request)
    {
        var result = await _updateSupplier.HandleAsync(id, request);

        if (!result.Success)
        {
            var response = ApiResponse<SupplierResponse>.Fail(result.ErrorMessage!, result.ErrorCode);
            return result.ErrorCode == "suppliers_not_found"
                ? NotFound(response)
                : BadRequest(response);
        }

        return Ok(ApiResponse<SupplierResponse>.Ok(result.Value!, "Supplier updated successfully."));
    }
}
