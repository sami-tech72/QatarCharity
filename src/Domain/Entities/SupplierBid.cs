namespace Domain.Entities;

public class SupplierBid
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RfxId { get; set; }

    public string SubmittedByUserId { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public string ProposalSummary { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string DocumentsJson { get; set; } = string.Empty;

    public string InputsJson { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public string EvaluationStatus { get; set; } = "Pending Review";

    public string? EvaluationNotes { get; set; }

    public DateTime? EvaluatedAtUtc { get; set; }

    public string? EvaluatedByUserId { get; set; }
}
