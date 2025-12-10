using Api.Models;
using Application.DTOs.Common;
using Application.DTOs.Contracts;
using Application.Features.ContractManagement.Queries;
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
    private readonly GetContractReadyBidsQuery _getContractReadyBidsQuery;

    public ContractManagementController(GetContractReadyBidsQuery getContractReadyBidsQuery)
    {
        _getContractReadyBidsQuery = getContractReadyBidsQuery;
    }

    [HttpGet("ready")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContractReadyBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractReadyBidResponse>>>> GetContractReadyBids([FromQuery] ContractReadyQueryParameters query)
    {
        var result = await _getContractReadyBidsQuery.HandleAsync(query);

        return Ok(ApiResponse<PagedResult<ContractReadyBidResponse>>.Ok(result.Value!, "Approved bids ready for contract management retrieved."));
    }
}
