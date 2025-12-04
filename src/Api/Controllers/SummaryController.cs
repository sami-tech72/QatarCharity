using Api.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class SummaryController : ControllerBase
{
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetAdminSummary()
    {
        var summary = new
        {
            Message = "Welcome, Admin",
            Timestamp = DateTimeOffset.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(summary, "Summary retrieved successfully."));
    }

    [Authorize(Roles = $"{Roles.Procurement},{Roles.CommitteeMember}")]
    [HttpGet("procurement/summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetProcurementSummary()
    {
        var summary = new
        {
            Message = "Welcome, Procurement",
            Timestamp = DateTimeOffset.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(summary, "Summary retrieved successfully."));
    }

    [Authorize(Roles = Roles.Supplier)]
    [HttpGet("supplier/summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetSupplierSummary()
    {
        var summary = new
        {
            Message = "Welcome, Supplier",
            Timestamp = DateTimeOffset.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(summary, "Summary retrieved successfully."));
    }
}
