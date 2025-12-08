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
    decimal BidAmount,
    string Currency,
    DateTime? ExpectedDeliveryDate,
    DateTime SubmittedAtUtc,
    string ProposalSummary,
    string? Notes);
