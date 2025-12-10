using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Contracts;
using Application.Features.ContractManagement.Commands;
using Application.Features.ContractManagement.Queries;
using Api.Authorization;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Procurement}")]
public class ContractManagementController : ControllerBase
{
    private readonly CreateContractCommand _createContractCommand;
    private readonly GetContractReadyBidsQuery _getContractReadyBidsQuery;
    private readonly GetContractsQuery _getContractsQuery;

    public ContractManagementController(
        GetContractReadyBidsQuery getContractReadyBidsQuery,
        GetContractsQuery getContractsQuery,
        CreateContractCommand createContractCommand)
    {
        _getContractReadyBidsQuery = getContractReadyBidsQuery;
        _getContractsQuery = getContractsQuery;
        _createContractCommand = createContractCommand;
    }

    [HttpGet]
    [Authorize(Policy = ProcurementPolicies.ContractManagementRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContractResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractResponse>>>> GetContracts([FromQuery] ContractReadyQueryParameters query)
    {
        var result = await _getContractsQuery.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<ContractResponse>>.Ok(result.Value!, "Contracts retrieved."));
    }

    [HttpGet("ready")]
    [Authorize(Policy = ProcurementPolicies.ContractManagementRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContractReadyBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractReadyBidResponse>>>> GetContractReadyBids([FromQuery] ContractReadyQueryParameters query)
    {
        var result = await _getContractReadyBidsQuery.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<ContractReadyBidResponse>>.Ok(result.Value!, "Approved bids ready for contract management retrieved."));
    }

    [HttpPost]
    [Authorize(Policy = ProcurementPolicies.ContractManagementCreate)]
    [ProducesResponseType(typeof(ApiResponse<ContractResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ContractResponse>>> CreateContract([FromBody] CreateContractRequest request)
    {
        var result = await _createContractCommand.HandleAsync(request);

        if (!result.Success)
        {
            var message = result.ErrorMessage ?? "Contract creation failed.";
            return BadRequest(ApiResponse<ContractResponse>.Fail(message, result.ErrorCode));
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<ContractResponse>.Ok(result.Value!, "Contract created."));
    }
}
