using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Rfx.Commands;

public class EvaluateBidCommand(IRfxRepository repository, IUserDirectoryService userDirectoryService)
{
    public async Task<Result<SupplierBidResponse>> HandleAsync(
        Guid rfxId,
        Guid bidId,
        EvaluateBidRequest request,
        string currentUserId)
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

        // 🔥 1) Upsert per-user review (committee member)
        var existingReview = await repository.GetBidReviewForUserAsync(bid.Id, currentUserId);

        var notes = string.IsNullOrWhiteSpace(request.ReviewNotes)
            ? null
            : request.ReviewNotes.Trim();

        if (existingReview is null)
        {
            var newReview = new SupplierBidReview
            {
                BidId = bid.Id,
                ReviewerUserId = currentUserId,
                Status = normalizedStatus,
                Notes = notes,
                ReviewedAtUtc = DateTime.UtcNow
            };

            await repository.AddBidReviewAsync(newReview);
        }
        else
        {
            existingReview.Status = normalizedStatus;
            existingReview.Notes = notes;
            existingReview.ReviewedAtUtc = DateTime.UtcNow;
        }

        // 🔥 2) Still maintain overall bid evaluation (for summary / search)
        bid.EvaluationStatus = normalizedStatus;
        bid.EvaluationNotes = notes;
        bid.EvaluatedAtUtc = DateTime.UtcNow;
        bid.EvaluatedByUserId = currentUserId;

        await repository.SaveChangesAsync();

        // 🔥 3) Load all reviews for this bid
        var allReviews = await repository.GetBidReviewsAsync(bid.Id);

        // 🔥 4) Build user lookup: submitted by + overall evaluated by + all reviewers
        var userIds = new List<string>
        {
            bid.SubmittedByUserId,
            bid.EvaluatedByUserId ?? string.Empty
        };

        userIds.AddRange(allReviews.Select(r => r.ReviewerUserId));

        userIds = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var userLookup = await userDirectoryService.GetUserNamesAsync(userIds);

        // 🔥 5) Map to response including committee reviews
        var response = RfxMapping.BuildBidResponse(bid, rfx, userLookup, allReviews);

        return Result<SupplierBidResponse>.Ok(response);
    }
}
