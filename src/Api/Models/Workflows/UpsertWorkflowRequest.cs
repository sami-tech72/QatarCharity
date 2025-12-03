using System.Collections.Generic;

namespace Api.Models.Workflows;

public record UpsertWorkflowRequest(
    string Name,
    string? Description,
    string Status,
    IReadOnlyList<WorkflowStepRequest> Steps);
