namespace Application.DTOs.Contracts;

public record ContractReadyQueryParameters
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Search { get; init; }
}

public record ContractReadyBidResponse(
    Guid BidId,
    Guid RfxId,
    string ReferenceNumber,
    string Title,
    string SupplierName,
    decimal BidAmount,
    string Currency,
    string EvaluationStatus,
    DateTime SubmittedAtUtc,
    DateTime? EvaluatedAtUtc);
