using System;

namespace Domain.Entities.Procurement;

public class BidReview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BidId { get; set; }
        = Guid.Empty;

    public string ReviewerUserId { get; set; } = string.Empty;

    public string Decision { get; set; } = string.Empty;

    public string? Comments { get; set; }
        = null;

    public DateTime ReviewedAtUtc { get; set; } = DateTime.UtcNow;
}
