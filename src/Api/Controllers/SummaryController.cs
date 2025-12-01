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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetAdminSummary()
    {
        return Ok(new
        {
            Message = "Welcome, Admin",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    [Authorize(Roles = Roles.Procurement)]
    [HttpGet("procurement/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetProcurementSummary()
    {
        return Ok(new
        {
            Message = "Welcome, Procurement",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    [Authorize(Roles = Roles.Supplier)]
    [HttpGet("supplier/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetSupplierSummary()
    {
        return Ok(new
        {
            Message = "Welcome, Supplier",
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
