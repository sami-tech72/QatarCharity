using System;

namespace Domain.Entities;

public class RfxEvaluationCriterion
{
    public Guid Id { get; set; }

    public Guid RfxId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Weight { get; set; }
        = 0;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Rfx? Rfx { get; set; }
        = null;
}
