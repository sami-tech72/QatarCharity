using System;
using System.Collections.Generic;

namespace Api.Models.Workflows;

public record WorkflowDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int NumberOfStages,
    DateTime LastModified,
    IReadOnlyList<WorkflowStepResponse> Steps);
