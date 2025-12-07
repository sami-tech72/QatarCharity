using System;
using System.Collections.Generic;

namespace Api.Models.Rfx;

public record RfxQueryParameters
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Search { get; init; }
    public bool AssignedOnly { get; init; }
}

public record CreateRfxRequest
{
    public string RfxType { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal EstimatedBudget { get; init; }
        = 0m;
    public string Currency { get; init; } = string.Empty;
    public bool HideBudget { get; init; }
        = false;
    public DateTime PublicationDate { get; init; }
        = DateTime.UtcNow;
    public DateTime ClosingDate { get; init; }
        = DateTime.UtcNow;
    public DateTime SubmissionDeadline { get; init; }
        = DateTime.UtcNow;
    public string Priority { get; init; } = string.Empty;
    public bool TenderBondRequired { get; init; }
        = false;
    public string ContactPerson { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string TechnicalSpecification { get; init; } = string.Empty;
    public string Deliverables { get; init; } = string.Empty;
    public string Timeline { get; init; } = string.Empty;
    public List<string> RequiredDocuments { get; init; } = new();
    public List<RfxCriterionDto> EvaluationCriteria { get; init; } = new();
    public int MinimumScore { get; init; }
        = 0;
    public string? EvaluationNotes { get; init; }
        = null;
    public List<string> CommitteeMemberIds { get; init; } = new();
    public Guid? WorkflowId { get; init; }
        = null;
    public string Status { get; init; } = "Draft";
}

public record RfxCriterionDto(string Title, int Weight, string Description, string Type);

public record RfxSummaryResponse(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Category,
    string Status,
    string CommitteeStatus,
    DateTime ClosingDate,
    decimal EstimatedBudget,
    string Currency,
    string? WorkflowName,
    bool CanApprove);

public record RfxEvaluationCriterionResponse(
    Guid Id,
    string Title,
    int Weight,
    string Description,
    string Type);

public record RfxCommitteeMemberResponse(Guid Id, string DisplayName, string? UserId);

public record RfxDetailResponse(
    Guid Id,
    string ReferenceNumber,
    string RfxType,
    string Category,
    string Title,
    string Department,
    string Description,
    decimal EstimatedBudget,
    string Currency,
    bool HideBudget,
    DateTime PublicationDate,
    DateTime ClosingDate,
    DateTime SubmissionDeadline,
    string Priority,
    bool TenderBondRequired,
    string ContactPerson,
    string ContactEmail,
    string ContactPhone,
    string Scope,
    string TechnicalSpecification,
    string Deliverables,
    string Timeline,
    IReadOnlyCollection<string> RequiredDocuments,
    int MinimumScore,
    string? EvaluationNotes,
    string Status,
    Guid? WorkflowId,
    IReadOnlyCollection<RfxEvaluationCriterionResponse> EvaluationCriteria,
    IReadOnlyCollection<RfxCommitteeMemberResponse> CommitteeMembers,
    DateTime CreatedAt,
    DateTime LastModified,
    string? WorkflowName);
