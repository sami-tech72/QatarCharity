using System;

namespace Api.Models.Rfx;

public record SupplierBidResponse(
    Guid Id,
    Guid RfxId,
    string ReferenceNumber,
    string Title,
    string SubmittedBy,
    decimal BidAmount,
    string Currency,
    DateTime? ExpectedDeliveryDate,
    string ProposalSummary,
    string? Notes,
    DateTime SubmittedAtUtc,
    string EvaluationStatus,
    string? EvaluationNotes,
    DateTime? EvaluatedAtUtc,
    string? EvaluatedBy);

public record EvaluateBidRequest(string Status, string? ReviewNotes);

public class SupplierBidQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; } = string.Empty;
}
