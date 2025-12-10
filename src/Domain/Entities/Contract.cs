using System;

namespace Domain.Entities;

public class Contract
{
    public Guid Id { get; set; }

    public Guid BidId { get; set; }

    public Guid RfxId { get; set; }

    public string Title { get; set; } = default!;

    public string SupplierName { get; set; } = default!;

    public string SupplierUserId { get; set; } = default!;

    public decimal ContractValue { get; set; }

    public string Currency { get; set; } = "USD";

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public string Status { get; set; } = "Draft";

    public string? SupplierSignature { get; set; }

    public DateTime? SupplierSignedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
