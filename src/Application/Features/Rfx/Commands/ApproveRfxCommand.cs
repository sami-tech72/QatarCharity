using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Rfx.Commands;

public class ApproveRfxCommand(IRfxRepository repository)
{
    public async Task<Result<RfxDetailResponse>> HandleAsync(Guid id, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Result<RfxDetailResponse>.Fail("auth_invalid_token", "Invalid or expired token.");
        }

        var rfx = await repository.GetRfxWithEvaluationAsync(id);

        if (rfx is null)
        {
            return Result<RfxDetailResponse>.Fail("rfx_not_found", "RFx not found.");
        }

        var isAssignee = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId);

        if (!isAssignee)
        {
            return Result<RfxDetailResponse>.Fail("forbidden", "Current user cannot approve this RFx.");
        }

        if (!rfx.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_status", "Only RFx records in Draft status can be approved.");
        }

        var committeeMember = rfx.CommitteeMembers.FirstOrDefault(member => member.UserId == currentUserId);

        if (committeeMember is null)
        {
            return Result<RfxDetailResponse>.Fail("forbidden", "Current user cannot approve this RFx.");
        }

        if (committeeMember.IsApproved)
        {
            return Result<RfxDetailResponse>.Fail("rfx_already_approved", "You have already approved this RFx.");
        }

        committeeMember.IsApproved = true;
        rfx.LastModified = DateTime.UtcNow;

        if (rfx.CommitteeMembers.All(member => member.IsApproved))
        {
            rfx.Status = "Published";
        }

        await repository.SaveChangesAsync();

        var response = RfxMapping.MapToDetailResponse(rfx, rfx.Workflow?.Name);

        return Result<RfxDetailResponse>.Ok(response);
    }
}
