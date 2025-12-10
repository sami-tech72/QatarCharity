using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Contracts;
using Application.Features.ContractManagement.Commands;
using Application.Features.ContractManagement.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/supplier/contracts")]
[Authorize(Roles = Roles.Supplier)]
public class SupplierContractsController : ControllerBase
{
    private readonly GetSupplierContractsQuery _getSupplierContractsQuery;
    private readonly SignContractCommand _signContractCommand;

    public SupplierContractsController(
        GetSupplierContractsQuery getSupplierContractsQuery,
        SignContractCommand signContractCommand)
    {
        _getSupplierContractsQuery = getSupplierContractsQuery;
        _signContractCommand = signContractCommand;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierContractResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierContractResponse>>>> GetMyContracts(
        [FromQuery] SupplierContractQueryParameters query)
    {
        var currentUserId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<PagedResult<SupplierContractResponse>>.Fail("Unauthorized", "auth_invalid_token"));
        }

        var result = await _getSupplierContractsQuery.HandleAsync(currentUserId, query);
        return Ok(ApiResponse<PagedResult<SupplierContractResponse>>.Ok(result.Value!, "Supplier contracts retrieved successfully."));
    }

    [HttpPost("{contractId:guid}/sign")]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContractResponse>>> SignContract(Guid contractId, [FromBody] SignContractRequest request)
    {
        var currentUserId = User?.FindFirst("sub")?.Value ?? User?.Identity?.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<ContractResponse>.Fail("Unauthorized", "auth_invalid_token"));
        }

        var result = await _signContractCommand.HandleAsync(contractId, currentUserId, request);
        if (!result.Success)
        {
            var status = result.ErrorCode switch
            {
                "not_found" => StatusCodes.Status404NotFound,
                "invalid_status" => StatusCodes.Status409Conflict,
                "invalid_signature" => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest,
            };

            return StatusCode(status, ApiResponse<ContractResponse>.Fail(result.ErrorMessage ?? "Unable to sign contract.", result.ErrorCode));
        }

        return Ok(ApiResponse<ContractResponse>.Ok(result.Value!, "Contract signed successfully."));
    }
}
