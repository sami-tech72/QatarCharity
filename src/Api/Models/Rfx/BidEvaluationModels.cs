using System;

namespace Api.Models.Rfx;

public record BidQueryParameters
{
    public int PageNumber { get; init; }
        = 1;

    public int PageSize { get; init; }
        = 10;

    public string? Search { get; init; }
        = null;
}

public record SupplierBidSummaryResponse(
    Guid Id,
    Guid RfxId,
    string RfxReferenceNumber,
    string RfxTitle,
    string SupplierName,
    string SubmittedByUserId,
    string SubmittedByName,
    decimal BidAmount,
    string Currency,
    DateTime? ExpectedDeliveryDate,
    DateTime SubmittedAtUtc,
    string ProposalSummary,
    string? Notes,
    IReadOnlyList<BidReviewResponse> Reviews);

public record BidReviewResponse(
    Guid Id,
    Guid BidId,
    string ReviewerUserId,
    string ReviewerName,
    string Decision,
    DateTime ReviewedAtUtc,
    string? Comments);

public record ReviewBidRequest
{
    public string Decision { get; init; } = string.Empty;

    public string? Comments { get; init; }
        = null;
}
