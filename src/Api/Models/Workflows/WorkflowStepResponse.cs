using System;

namespace Api.Models.Workflows;

public record WorkflowStepResponse(
    Guid Id,
    string Name,
    string StepType,
    string? AssigneeId,
    string? AssigneeName,
    int Order);
