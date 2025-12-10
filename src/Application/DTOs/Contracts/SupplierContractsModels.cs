using System;

namespace Application.DTOs.Contracts;

public record SupplierContractQueryParameters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
}

public record SupplierContractResponse(
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

public record SignContractRequest
{
    public string Signature { get; init; } = string.Empty;
}
