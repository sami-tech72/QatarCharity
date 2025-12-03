using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Workflow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Draft";

    public DateTime LastModified { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<WorkflowStep> Steps { get; set; } = new();
}
