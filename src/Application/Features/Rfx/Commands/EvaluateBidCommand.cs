using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;

namespace Application.Features.Rfx.Commands;

public class EvaluateBidCommand(IRfxRepository repository, IUserDirectoryService userDirectoryService)
{
    public async Task<Result<SupplierBidResponse>> HandleAsync(Guid rfxId, Guid bidId, EvaluateBidRequest request, string currentUserId)
    {
        var validation = RfxValidation.ValidateBidStatus(request);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedStatus = RfxValidation.NormalizeBidStatus(request.Status)!;

        var rfx = await repository.GetRfxByIdAsync(rfxId);
        if (rfx is null)
        {
            return Result<SupplierBidResponse>.Fail("rfx_not_found", "Tender not found.");
        }

        var bid = await repository.GetBidAsync(rfxId, bidId);
        if (bid is null)
        {
            return Result<SupplierBidResponse>.Fail("bid_not_found", "Bid not found for this tender.");
        }

        bid.EvaluationStatus = normalizedStatus;
        bid.EvaluationNotes = string.IsNullOrWhiteSpace(request.ReviewNotes) ? null : request.ReviewNotes.Trim();
        bid.EvaluatedAtUtc = DateTime.UtcNow;
        bid.EvaluatedByUserId = currentUserId;

        await repository.SaveChangesAsync();

        var userLookup = await userDirectoryService.GetUserNamesAsync(new[] { bid.SubmittedByUserId, bid.EvaluatedByUserId ?? string.Empty });
        var response = RfxMapping.BuildBidResponse(bid, rfx, userLookup);

        return Result<SupplierBidResponse>.Ok(response);
    }
}
