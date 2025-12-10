namespace Domain.Entities;

public class SupplierBidReview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BidId { get; set; }
    public SupplierBid Bid { get; set; } = default!;

    public string ReviewerUserId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime ReviewedAtUtc { get; set; } = DateTime.UtcNow;
}
