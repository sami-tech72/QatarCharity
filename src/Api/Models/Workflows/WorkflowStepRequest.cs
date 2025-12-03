namespace Api.Models.Workflows;

public record WorkflowStepRequest(
    string Name,
    string StepType,
    string? AssigneeId,
    int Order);
