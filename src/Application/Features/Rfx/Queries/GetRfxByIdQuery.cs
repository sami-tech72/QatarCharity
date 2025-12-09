using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Rfx.Queries;

public class GetRfxByIdQuery(IRfxRepository repository)
{
    public async Task<Result<RfxDetailResponse>> HandleAsync(Guid id, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Result<RfxDetailResponse>.Fail("auth_invalid_token", "Invalid or expired token.");
        }

        if (id == Guid.Empty)
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_id", "A valid RFx identifier is required.");
        }

        var rfx = await repository.GetRfxWithEvaluationAsync(id);

        if (rfx is null)
        {
            return Result<RfxDetailResponse>.Fail("rfx_not_found", "RFx not found.");
        }

        var response = RfxMapping.MapToDetailResponse(rfx, rfx.Workflow?.Name);
        return Result<RfxDetailResponse>.Ok(response);
    }
}
