using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Rfx;

public class RfxService(IRfxRepository repository, IUserDirectoryService userDirectoryService) : IRfxService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Published",
        "Closed",
    };

    private static readonly HashSet<string> AllowedBidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending Review",
        "Under Review",
        "Recommended",
        "Approved",
        "Rejected",
        "Needs Clarification",
    };

    public async Task<Result<PagedResult<SupplierBidResponse>>> GetSupplierBidsAsync(SupplierBidQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountSupplierBidsAsync(search);
        var results = await repository.GetSupplierBidsAsync(search, pageNumber, pageSize);

        var userLookup = await userDirectoryService.GetUserNamesAsync(results
            .Select(entry => entry.Bid.SubmittedByUserId)
            .Concat(results.Select(entry => entry.Bid.EvaluatedByUserId ?? string.Empty))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct());

        var bidResponses = results
            .Select(entry => BuildBidResponse(entry.Bid, entry.Rfx, userLookup))
            .ToList();

        var pagedResult = new PagedResult<SupplierBidResponse>(bidResponses, totalCount, pageNumber, pageSize);

        return Result<PagedResult<SupplierBidResponse>>.Ok(pagedResult);
    }

    public async Task<Result<PagedResult<RfxSummaryResponse>>> GetRfxListAsync(string currentUserId, RfxQueryParameters query)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Result<PagedResult<RfxSummaryResponse>>.Fail("auth_invalid_token", "Invalid or expired token.");
        }

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountRfxAsync(search, query.AssignedOnly, currentUserId);
        var rfxes = await repository.GetRfxSummariesAsync(search, query.AssignedOnly, currentUserId, pageNumber, pageSize);

        var summaries = rfxes
            .Select(rfx => new
            {
                Entity = rfx,
                CommitteeCount = rfx.CommitteeMembers.Count,
                ApprovedCount = rfx.CommitteeMembers.Count(member => member.IsApproved),
                CanApprove = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId),
            })
            .Select(entry => new RfxSummaryResponse(
                entry.Entity.Id,
                entry.Entity.ReferenceNumber,
                entry.Entity.Title,
                entry.Entity.Category,
                entry.Entity.Status,
                entry.Entity.Status.Equals("Published", StringComparison.OrdinalIgnoreCase)
                    ? "Approved"
                    : entry.CommitteeCount > 0
                        ? $"{entry.ApprovedCount}/{entry.CommitteeCount} Approved"
                        : "Pending",
                entry.Entity.ClosingDate,
                entry.Entity.EstimatedBudget,
                entry.Entity.Currency,
                entry.Entity.Workflow?.Name,
                entry.CanApprove))
            .ToList();

        var pagedResult = new PagedResult<RfxSummaryResponse>(summaries, totalCount, pageNumber, pageSize);
        return Result<PagedResult<RfxSummaryResponse>>.Ok(pagedResult);
    }

    public async Task<Result<RfxDetailResponse>> CreateRfxAsync(CreateRfxRequest request)
    {
        var validationResult = await ValidateRequestAsync(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var workflow = request.WorkflowId.HasValue
            ? await repository.GetWorkflowAsync(request.WorkflowId.Value)
            : null;

        var referenceNumber = await repository.GenerateReferenceNumberAsync();
        var now = DateTime.UtcNow;

        var rfx = new Domain.Entities.Rfx
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            RfxType = request.RfxType.Trim(),
            Category = request.Category.Trim(),
            Title = request.Title.Trim(),
            Department = request.Department.Trim(),
            Description = request.Description.Trim(),
            EstimatedBudget = request.EstimatedBudget,
            Currency = request.Currency.Trim(),
            HideBudget = request.HideBudget,
            PublicationDate = request.PublicationDate,
            ClosingDate = request.ClosingDate,
            SubmissionDeadline = request.SubmissionDeadline,
            Priority = request.Priority.Trim(),
            TenderBondRequired = request.TenderBondRequired,
            ContactPerson = request.ContactPerson.Trim(),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Scope = request.Scope.Trim(),
            TechnicalSpecification = request.TechnicalSpecification.Trim(),
            Deliverables = request.Deliverables.Trim(),
            Timeline = request.Timeline.Trim(),
            RequiredDocuments = SerializeList(request.RequiredDocuments),
            MinimumScore = request.MinimumScore,
            EvaluationNotes = string.IsNullOrWhiteSpace(request.EvaluationNotes)
                ? null
                : request.EvaluationNotes.Trim(),
            Status = NormalizeStatus(request.Status),
            WorkflowId = workflow?.Id,
            CreatedAt = now,
            LastModified = now,
            EvaluationCriteria = request.EvaluationCriteria
                .Select(criterion => new RfxEvaluationCriterion
                {
                    Id = Guid.NewGuid(),
                    Title = criterion.Title.Trim(),
                    Weight = criterion.Weight,
                    Description = criterion.Description.Trim(),
                    Type = criterion.Type.Trim(),
                })
                .ToList(),
            CommitteeMembers = await repository.BuildCommitteeMembersAsync(request.CommitteeMemberIds),
        };

        await repository.AddRfxAsync(rfx);
        await repository.SaveChangesAsync();

        var response = MapToDetailResponse(rfx, workflow?.Name);

        return Result<RfxDetailResponse>.Ok(response);
    }

    public async Task<Result<RfxDetailResponse>> GetRfxByIdAsync(Guid id, string currentUserId)
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

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);
        return Result<RfxDetailResponse>.Ok(response);
    }

    public async Task<Result<SupplierBidResponse>> EvaluateBidAsync(Guid rfxId, Guid bidId, EvaluateBidRequest request, string currentUserId)
    {
        if (request is null)
        {
            return Result<SupplierBidResponse>.Fail("invalid_request", "Review details are required.");
        }

        var normalizedStatus = AllowedBidStatuses.FirstOrDefault(status => status.Equals(request.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedStatus is null)
        {
            return Result<SupplierBidResponse>.Fail("invalid_status", "Invalid bid status provided.");
        }

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
        var response = BuildBidResponse(bid, rfx, userLookup);

        return Result<SupplierBidResponse>.Ok(response);
    }

    public async Task<Result<RfxDetailResponse>> ApproveRfxAsync(Guid id, string currentUserId)
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

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);

        return Result<RfxDetailResponse>.Ok(response);
    }

    public async Task<Result<RfxDetailResponse>> CloseRfxAsync(Guid id, string currentUserId, bool isAdmin)
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

        if (!isAdmin && !isAssignee)
        {
            return Result<RfxDetailResponse>.Fail("forbidden", "Current user cannot close this RFx.");
        }

        if (rfx.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
        {
            return Result<RfxDetailResponse>.Fail("rfx_already_closed", "This RFx is already closed.");
        }

        rfx.Status = "Closed";
        rfx.LastModified = DateTime.UtcNow;

        await repository.SaveChangesAsync();

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);
        return Result<RfxDetailResponse>.Ok(response);
    }

    private async Task<Result<RfxDetailResponse>?> ValidateRequestAsync(CreateRfxRequest request)
    {
        if (request is null)
        {
            return Result<RfxDetailResponse>.Fail("invalid_request", "A valid RFx payload is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_title", "A title is required.");
        }

        var invalidCommitteeMembers = await repository.GetMissingCommitteeMemberIdsAsync(request.CommitteeMemberIds);
        if (invalidCommitteeMembers.Count > 0)
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_committee", "One or more committee members are invalid.");
        }

        var normalizedStatus = NormalizeStatus(request.Status);
        if (!AllowedStatuses.Contains(normalizedStatus))
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_status", "Invalid RFx status provided.");
        }

        if (request.ClosingDate <= DateTime.UtcNow)
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_dates", "Closing date must be in the future.");
        }

        return null;
    }

    private static string SerializeList(IEnumerable<string> values)
    {
        var sanitized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());

        return string.Join(';', sanitized);
    }

    private static string NormalizeStatus(string status)
    {
        return AllowedStatuses.FirstOrDefault(state => state.Equals(status?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? "Draft";
    }

    private static SupplierBidResponse BuildBidResponse(SupplierBid bid, Domain.Entities.Rfx rfx, IReadOnlyDictionary<string, string> userLookup)
    {
        userLookup.TryGetValue(bid.SubmittedByUserId, out var submittedBy);
        userLookup.TryGetValue(bid.EvaluatedByUserId ?? string.Empty, out var evaluatedBy);

        return new SupplierBidResponse(
            bid.Id,
            bid.RfxId,
            rfx.ReferenceNumber,
            rfx.Title,
            submittedBy ?? bid.SubmittedByUserId,
            bid.BidAmount,
            bid.Currency,
            bid.ExpectedDeliveryDate,
            bid.ProposalSummary,
            bid.Notes,
            bid.SubmittedAtUtc,
            bid.EvaluationStatus,
            bid.EvaluationNotes,
            bid.EvaluatedAtUtc,
            evaluatedBy);
    }

    private static RfxDetailResponse MapToDetailResponse(Domain.Entities.Rfx rfx, string? workflowName)
    {
        var requiredDocuments = DeserializeList(rfx.RequiredDocuments);

        var criteria = rfx.EvaluationCriteria
            .OrderBy(criterion => criterion.Type)
            .ThenBy(criterion => criterion.Title)
            .Select(criterion => new RfxEvaluationCriterionResponse(
                criterion.Id,
                criterion.Title,
                criterion.Weight,
                criterion.Description,
                criterion.Type))
            .ToList();

        var committeeMembers = rfx.CommitteeMembers
            .OrderBy(member => member.DisplayName)
            .Select(member => new RfxCommitteeMemberResponse(
                member.Id,
                member.DisplayName,
                member.UserId,
                member.IsApproved))
            .ToList();

        return new RfxDetailResponse(
            rfx.Id,
            rfx.ReferenceNumber,
            rfx.RfxType,
            rfx.Category,
            rfx.Title,
            rfx.Department,
            rfx.Description,
            rfx.EstimatedBudget,
            rfx.Currency,
            rfx.HideBudget,
            rfx.PublicationDate,
            rfx.ClosingDate,
            rfx.SubmissionDeadline,
            rfx.Priority,
            rfx.TenderBondRequired,
            rfx.ContactPerson,
            rfx.ContactEmail,
            rfx.ContactPhone,
            rfx.Scope,
            rfx.TechnicalSpecification,
            rfx.Deliverables,
            rfx.Timeline,
            requiredDocuments,
            rfx.MinimumScore,
            rfx.EvaluationNotes,
            rfx.Status,
            rfx.WorkflowId,
            criteria,
            committeeMembers,
            rfx.CreatedAt,
            rfx.LastModified,
            workflowName);
    }

    private static List<string> DeserializeList(string data)
    {
        return string.IsNullOrWhiteSpace(data)
            ? new List<string>()
            : data.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();
    }
}
