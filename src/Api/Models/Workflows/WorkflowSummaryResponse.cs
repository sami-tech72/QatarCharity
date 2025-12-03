using System;

namespace Api.Models.Workflows;

public record WorkflowSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int NumberOfStages,
    DateTime LastModified);
