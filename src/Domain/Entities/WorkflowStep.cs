using System;

namespace Domain.Entities;

public class WorkflowStep
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string StepType { get; set; } = string.Empty;

    public string? AssigneeId { get; set; }

    public int Order { get; set; }

    public Workflow? Workflow { get; set; }

    public ApplicationUser? Assignee { get; set; }
}
