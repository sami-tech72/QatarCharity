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
    string SupplierUserId,
    string SupplierName,
    decimal BidAmount,
    string Currency,
    string EvaluationStatus,
    DateTime SubmittedAtUtc,
    DateTime? EvaluatedAtUtc);

public record CreateContractRequest
{
    public Guid? BidId { get; init; }
    public Guid? RfxId { get; init; }
    public bool IsDirectContract { get; init; }
    public string Title { get; init; } = string.Empty;
    public string SupplierName { get; init; } = string.Empty;
    public string SupplierUserId { get; init; } = string.Empty;
    public decimal ContractValue { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime StartDateUtc { get; init; }
    public DateTime EndDateUtc { get; init; }
    public string Status { get; init; } = "Draft";
}

public record ContractResponse(
    Guid Id,
    Guid? BidId,
    Guid? RfxId,
    string ReferenceNumber,
    string Title,
    string SupplierName,
    string SupplierUserId,
    decimal ContractValue,
    string Currency,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    string Status,
    DateTime CreatedAtUtc,
    string? SupplierSignature,
    DateTime? SupplierSignedAtUtc);

public record ContractCompanyDetails(
    string Name,
    string Address,
    string ContactEmail,
    string ContactPhone);

public record ContractSupplierDetails(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    string CompanyAddress,
    string SupplierName,
    string SupplierUserId);

public record ContractDetailResponse(
    Guid Id,
    Guid? BidId,
    Guid? RfxId,
    string ReferenceNumber,
    string Title,
    decimal ContractValue,
    string Currency,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    string Status,
    DateTime CreatedAtUtc,
    string? SupplierSignature,
    DateTime? SupplierSignedAtUtc,
    ContractSupplierDetails Supplier,
    ContractCompanyDetails IssuerCompany);
