using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Rfx
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string RfxType { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal EstimatedBudget { get; set; }
        = 0m;

    public string Currency { get; set; } = "QAR";

    public bool HideBudget { get; set; }
        = false;

    public DateTime PublicationDate { get; set; }
        = DateTime.UtcNow;

    public DateTime ClosingDate { get; set; }
        = DateTime.UtcNow;

    public DateTime SubmissionDeadline { get; set; }
        = DateTime.UtcNow;

    public string Priority { get; set; } = "High";

    public bool TenderBondRequired { get; set; }
        = true;

    public string ContactPerson { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public string TechnicalSpecification { get; set; } = string.Empty;

    public string Deliverables { get; set; } = string.Empty;

    public string Timeline { get; set; } = string.Empty;

    public string RequiredDocuments { get; set; } = string.Empty;

    public int MinimumScore { get; set; }
        = 0;

    public string? EvaluationNotes { get; set; }
        = null;

    public string Status { get; set; } = "Draft";

    public Guid? WorkflowId { get; set; }
        = null;

    public Workflow? Workflow { get; set; }
        = null;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime LastModified { get; set; }
        = DateTime.UtcNow;

    public List<RfxEvaluationCriterion> EvaluationCriteria { get; set; } = new();

    public List<RfxCommitteeMember> CommitteeMembers { get; set; } = new();
}
