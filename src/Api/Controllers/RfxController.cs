using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Models;
using Api.Models.Rfx;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Controllers;

[ApiController]
[Route("api/rfx")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Procurement}")]
public class RfxController : ControllerBase
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Published",
        "Closed",
    };

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public RfxController(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RfxSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RfxSummaryResponse>>>> GetRfxList([FromQuery] RfxQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var rfxQuery = _dbContext.Rfxes
            .AsNoTracking()
            .Include(rfx => rfx.Workflow)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            rfxQuery = rfxQuery.Where(rfx =>
                rfx.ReferenceNumber.ToLower().Contains(search) ||
                rfx.Title.ToLower().Contains(search) ||
                rfx.Category.ToLower().Contains(search));
        }

        var totalCount = await rfxQuery.CountAsync();

        var rfxes = await rfxQuery
            .OrderByDescending(rfx => rfx.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(rfx => new
            {
                Entity = rfx,
                CommitteeCount = rfx.CommitteeMembers.Count,
            })
            .ToListAsync();

        var summaries = rfxes
            .Select(entry => new RfxSummaryResponse(
                entry.Entity.Id,
                entry.Entity.ReferenceNumber,
                entry.Entity.Title,
                entry.Entity.Category,
                entry.Entity.Status,
                entry.CommitteeCount > 0 ? "Assigned" : "Pending",
                entry.Entity.ClosingDate,
                entry.Entity.EstimatedBudget,
                entry.Entity.Currency,
                entry.Entity.Workflow?.Name))
            .ToList();

        var pagedResult = new PagedResult<RfxSummaryResponse>(summaries, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<RfxSummaryResponse>>.Ok(pagedResult, "RFx records retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> CreateRfx(CreateRfxRequest request)
    {
        var validationResult = await ValidateRequestAsync(request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var workflow = request.WorkflowId.HasValue
            ? await _dbContext.Workflows.AsNoTracking().FirstOrDefaultAsync(wf => wf.Id == request.WorkflowId.Value)
            : null;

        var referenceNumber = await GenerateReferenceNumberAsync();
        var now = DateTime.UtcNow;

        var rfx = new Rfx
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
            RequiredDocuments = SerializeList(request.RequiredDocuments),
            MinimumScore = request.MinimumScore,
            EvaluationNotes = string.IsNullOrWhiteSpace(request.EvaluationNotes)
                ? null
                : request.EvaluationNotes.Trim(),
            Status = NormalizeStatus(request.Status),
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
            CommitteeMembers = await MapCommitteeMembersAsync(request.CommitteeMemberIds),
        };

        _dbContext.Rfxes.Add(rfx);
        await _dbContext.SaveChangesAsync();

        var response = MapToDetailResponse(rfx, workflow?.Name);

        return Ok(ApiResponse<RfxDetailResponse>.Ok(response, "RFx created successfully."));
    }

    private async Task<ActionResult<ApiResponse<RfxDetailResponse>>?> ValidateRequestAsync(CreateRfxRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RfxType) || string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Department))
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "RFx type, title, category and department are required.",
                "rfx_required_fields"));
        }

        if (string.IsNullOrWhiteSpace(request.Status) || !AllowedStatuses.Contains(request.Status))
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "Status must be Draft, Published, or Closed.",
                "rfx_invalid_status"));
        }

        if (request.EvaluationCriteria.Count == 0)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "Please include at least one evaluation criterion.",
                "rfx_criteria_required"));
        }

        var totalWeight = request.EvaluationCriteria.Sum(c => c.Weight);

        if (totalWeight <= 0)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "Evaluation criteria must have a combined weight greater than zero.",
                "rfx_invalid_weights"));
        }

        if (request.MinimumScore is < 0 or > 100)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "Minimum score must be between 0 and 100.",
                "rfx_invalid_minimum_score"));
        }

        if (request.WorkflowId.HasValue)
        {
            var workflowExists = await _dbContext.Workflows
                .AsNoTracking()
                .AnyAsync(workflow => workflow.Id == request.WorkflowId.Value);

            if (!workflowExists)
            {
                return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                    "The selected workflow could not be found.",
                    "rfx_workflow_missing"));
            }
        }

        var missingAssignees = await ValidateCommitteeAsync(request.CommitteeMemberIds);

        if (missingAssignees.Count > 0)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "One or more committee members do not exist.",
                "rfx_committee_missing",
                new Dictionary<string, object?>
                {
                    ["missingCommitteeMembers"] = missingAssignees,
                }));
        }

        var normalizedTitle = request.Title.Trim();
        var duplicateExists = await _dbContext.Rfxes
            .AnyAsync(rfx => rfx.Title == normalizedTitle);

        if (duplicateExists)
        {
            return Conflict(ApiResponse<RfxDetailResponse>.Fail(
                "An RFx with the same title already exists.",
                "rfx_duplicate_title"));
        }

        return null;
    }

    private async Task<List<string>> ValidateCommitteeAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new List<string>();
        }

        var existingIds = await _userManager.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync();

        return ids.Except(existingIds).ToList();
    }

    private async Task<List<RfxCommitteeMember>> MapCommitteeMembersAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new List<RfxCommitteeMember>();
        }

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.Select(user => new RfxCommitteeMember
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DisplayName = user.DisplayName ?? user.Email ?? user.UserName ?? string.Empty,
        }).ToList();
    }

    private static RfxDetailResponse MapToDetailResponse(Rfx rfx, string? workflowName)
    {
        var requiredDocuments = DeserializeList(rfx.RequiredDocuments);

        var criteria = rfx.EvaluationCriteria
            .OrderBy(criterion => criterion.Type)
            .ThenBy(criterion => criterion.Title)
            .Select(criterion => new RfxEvaluationCriterionResponse(
                criterion.Id,
                criterion.Title,
                criterion.Weight,
                criterion.Description,
                criterion.Type))
            .ToList();

        var committee = rfx.CommitteeMembers
            .Select(member => new RfxCommitteeMemberResponse(member.Id, member.DisplayName, member.UserId))
            .ToList();

        return new RfxDetailResponse(
            rfx.Id,
            rfx.ReferenceNumber,
            rfx.RfxType,
            rfx.Category,
            rfx.Title,
            rfx.Department,
            rfx.Description,
            rfx.EstimatedBudget,
            rfx.Currency,
            rfx.HideBudget,
            rfx.PublicationDate,
            rfx.ClosingDate,
            rfx.SubmissionDeadline,
            rfx.Priority,
            rfx.TenderBondRequired,
            rfx.ContactPerson,
            rfx.ContactEmail,
            rfx.ContactPhone,
            rfx.Scope,
            rfx.TechnicalSpecification,
            rfx.Deliverables,
            rfx.Timeline,
            requiredDocuments,
            rfx.MinimumScore,
            rfx.EvaluationNotes,
            rfx.Status,
            rfx.WorkflowId,
            criteria,
            committee,
            rfx.CreatedAt,
            rfx.LastModified,
            workflowName);
    }

    private static string NormalizeStatus(string status)
    {
        return AllowedStatuses.First(value => value.Equals(status, StringComparison.OrdinalIgnoreCase));
    }

    private static string SerializeList(IEnumerable<string> values)
    {
        return JsonSerializer.Serialize(values?.Where(value => !string.IsNullOrWhiteSpace(value)).ToList() ?? new List<string>());
    }

    private static List<string> DeserializeList(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return new List<string>();
        }

        try
        {
            var values = JsonSerializer.Deserialize<List<string>>(serialized);
            return values ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<string> GenerateReferenceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var sequence = await _dbContext.Rfxes.CountAsync(rfx => rfx.CreatedAt.Year == year) + 1;
        return $"RFX-{year}-{sequence:D4}";
    }
}
