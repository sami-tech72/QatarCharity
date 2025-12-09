using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Domain.Entities;

namespace Application.Features.Rfx.Common;

internal static class RfxMapping
{
    public static SupplierBidResponse BuildBidResponse(SupplierBid bid, Rfx rfx, IReadOnlyDictionary<string, string> userLookup)
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

    public static RfxDetailResponse MapToDetailResponse(Rfx rfx, string? workflowName)
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

    public static PublishedRfxResponse BuildPublishedRfxResponse(Rfx rfx)
    {
        return new PublishedRfxResponse(
            rfx.Id,
            rfx.ReferenceNumber,
            rfx.RfxType,
            rfx.Title,
            rfx.Category,
            rfx.Description,
            rfx.PublicationDate,
            rfx.SubmissionDeadline,
            rfx.ClosingDate,
            rfx.EstimatedBudget,
            rfx.Currency,
            rfx.HideBudget,
            rfx.Scope,
            rfx.TechnicalSpecification,
            rfx.Deliverables,
            rfx.Timeline,
            DeserializeList(rfx.RequiredDocuments),
            BuildRequirementDetails(rfx),
            BuildRequiredInputs(rfx));
    }

    public static List<string> DeserializeList(string data)
    {
        return string.IsNullOrWhiteSpace(data)
            ? new List<string>()
            : data.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();
    }

    public static string SerializeList(IEnumerable<string> values)
    {
        var sanitized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());

        return string.Join(';', sanitized);
    }

    public static IReadOnlyCollection<string> BuildRequirementDetails(Rfx rfx)
    {
        var details = new List<string>();

        if (!string.IsNullOrWhiteSpace(rfx.Scope))
        {
            details.Add(rfx.Scope);
        }

        if (!string.IsNullOrWhiteSpace(rfx.TechnicalSpecification))
        {
            details.Add(rfx.TechnicalSpecification);
        }

        if (!string.IsNullOrWhiteSpace(rfx.Deliverables))
        {
            details.Add(rfx.Deliverables);
        }

        if (!string.IsNullOrWhiteSpace(rfx.Timeline))
        {
            details.Add(rfx.Timeline);
        }

        return details;
    }

    public static IReadOnlyCollection<string> BuildRequiredInputs(Rfx rfx)
    {
        var inputs = new List<string>
        {
            "Bid amount",
            "Expected delivery date",
            "Proposal summary",
        };

        if (!string.IsNullOrWhiteSpace(rfx.TechnicalSpecification))
        {
            inputs.Add("Technical compliance notes");
        }

        if (!string.IsNullOrWhiteSpace(rfx.Deliverables))
        {
            inputs.Add("Delivery approach for required deliverables");
        }

        return inputs;
    }
}
