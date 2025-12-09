using System;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Rfx.Queries;

public class GetPublishedRfxByIdQuery(IRfxRepository repository)
{
    public async Task<Result<PublishedRfxResponse>> HandleAsync(Guid rfxId)
    {
        var rfx = await repository.GetRfxByIdAsync(rfxId);

        if (rfx is null)
        {
            return Result<PublishedRfxResponse>.Fail("rfx_not_found", "Tender not found.");
        }

        if (!string.Equals(rfx.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return Result<PublishedRfxResponse>.Fail("rfx_not_published", "This tender is not open for bids.");
        }

        var response = RfxMapping.BuildPublishedRfxResponse(rfx);
        return Result<PublishedRfxResponse>.Ok(response);
    }
}
