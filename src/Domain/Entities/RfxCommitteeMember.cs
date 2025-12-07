using System;

namespace Domain.Entities;

public class RfxCommitteeMember
{
    public Guid Id { get; set; }

    public Guid RfxId { get; set; }

    public string? UserId { get; set; }
        = null;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsApproved { get; set; }
        = false;

    public Rfx? Rfx { get; set; }
        = null;
}
