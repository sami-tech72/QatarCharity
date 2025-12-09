using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Rfx.Commands;

public class CreateRfxCommand(IRfxRepository repository)
{
    public async Task<Result<RfxDetailResponse>> HandleAsync(CreateRfxRequest request)
    {
        var missingCommitteeMembers = await repository.GetMissingCommitteeMemberIdsAsync(request.CommitteeMemberIds);
        var validationResult = RfxValidation.ValidateCreateRequest(request, missingCommitteeMembers);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var workflow = request.WorkflowId.HasValue
            ? await repository.GetWorkflowAsync(request.WorkflowId.Value)
            : null;

        var referenceNumber = await repository.GenerateReferenceNumberAsync();
        var now = DateTime.UtcNow;

        var rfx = new Domain.Entities.Rfx
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            RfxType = request.RfxType.Trim(),
            Category = request.Category.Trim(),
            Title = request.Title.Trim(),
            Department = request.Department.Trim(),
            Description = request.Description.Trim(),
            EstimatedBudget = request.EstimatedBudget,
            Currency = request.Currency.Trim(),
            HideBudget = request.HideBudget,
            PublicationDate = request.PublicationDate,
            ClosingDate = request.ClosingDate,
            SubmissionDeadline = request.SubmissionDeadline,
            Priority = request.Priority.Trim(),
            TenderBondRequired = request.TenderBondRequired,
            ContactPerson = request.ContactPerson.Trim(),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Scope = request.Scope.Trim(),
            TechnicalSpecification = request.TechnicalSpecification.Trim(),
            Deliverables = request.Deliverables.Trim(),
            Timeline = request.Timeline.Trim(),
            RequiredDocuments = RfxMapping.SerializeList(request.RequiredDocuments),
            MinimumScore = request.MinimumScore,
            EvaluationNotes = string.IsNullOrWhiteSpace(request.EvaluationNotes)
                ? null
                : request.EvaluationNotes.Trim(),
            Status = RfxValidation.NormalizeStatus(request.Status),
            WorkflowId = workflow?.Id,
            CreatedAt = now,
            LastModified = now,
            EvaluationCriteria = request.EvaluationCriteria
                .Select(criterion => new RfxEvaluationCriterion
                {
                    Id = Guid.NewGuid(),
                    Title = criterion.Title.Trim(),
                    Weight = criterion.Weight,
                    Description = criterion.Description.Trim(),
                    Type = criterion.Type.Trim(),
                })
                .ToList(),
            CommitteeMembers = await repository.BuildCommitteeMembersAsync(request.CommitteeMemberIds),
        };

        await repository.AddRfxAsync(rfx);
        await repository.SaveChangesAsync();

        var response = RfxMapping.MapToDetailResponse(rfx, workflow?.Name);

        return Result<RfxDetailResponse>.Ok(response);
    }
}
