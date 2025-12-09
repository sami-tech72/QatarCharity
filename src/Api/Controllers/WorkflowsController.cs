using System;
using System.Collections.Generic;
using System.Linq;
using Api.Models;
using Api.Models.Workflows;
using Application.DTOs.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Identity;

namespace Api.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
public class WorkflowsController : ControllerBase
{
    private static readonly Dictionary<string, string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Active"] = "Active",
        ["Draft"] = "Draft",
        ["Archived"] = "Archived",
    };

    private static readonly Dictionary<string, string> AllowedStepTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Approval"] = "Approval",
        ["Task"] = "Task",
        ["Notification"] = "Notification",
    };

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkflowsController(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkflowSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkflowSummaryResponse>>>> GetWorkflows(
        [FromQuery] WorkflowQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var workflowsQuery = _dbContext.Workflows
            .AsNoTracking()
            .Include(workflow => workflow.Steps)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            workflowsQuery = workflowsQuery.Where(workflow => workflow.Name.ToLower().Contains(search));
        }

        var totalCount = await workflowsQuery.CountAsync();

        var workflows = await workflowsQuery
            .OrderByDescending(workflow => workflow.LastModified)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var summaries = workflows
            .Select(workflow => new WorkflowSummaryResponse(
                workflow.Id,
                workflow.Name,
                workflow.Description,
                workflow.Status,
                workflow.Steps.Count,
                workflow.LastModified))
            .ToList();

        var pagedResult = new PagedResult<WorkflowSummaryResponse>(summaries, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<WorkflowSummaryResponse>>.Ok(pagedResult, "Workflows retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkflowDetailResponse>>> GetWorkflow(Guid id)
    {
        var workflow = await _dbContext.Workflows
            .Include(w => w.Steps)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow is null)
        {
            return NotFound(ApiResponse<WorkflowDetailResponse>.Fail("Workflow not found.", "workflow_not_found"));
        }

        var assigneeLookup = await BuildAssigneeLookupAsync(workflow.Steps);
        var response = MapToDetailResponse(workflow, assigneeLookup);

        return Ok(ApiResponse<WorkflowDetailResponse>.Ok(response, "Workflow retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<WorkflowDetailResponse>>> CreateWorkflow(UpsertWorkflowRequest request)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var assigneeValidation = await ValidateAssigneesAsync(request.Steps);

        if (assigneeValidation is not null)
        {
            return assigneeValidation;
        }

        var normalizedName = request.Name.Trim();
        var normalizedStatus = NormalizeStatus(request.Status);
        var duplicateNameExists = await _dbContext.Workflows
            .AnyAsync(workflow => workflow.Name == normalizedName);

        if (duplicateNameExists)
        {
            return Conflict(ApiResponse<WorkflowDetailResponse>.Fail(
                "A workflow with the same name already exists.",
                "workflow_duplicate_name"));
        }

        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = normalizedStatus,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            Steps = MapSteps(request.Steps).ToList(),
        };

        _dbContext.Workflows.Add(workflow);
        await _dbContext.SaveChangesAsync();

        var assigneeLookup = await BuildAssigneeLookupAsync(workflow.Steps);
        var response = MapToDetailResponse(workflow, assigneeLookup);

        return Ok(ApiResponse<WorkflowDetailResponse>.Ok(response, "Workflow created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDetailResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<WorkflowDetailResponse>>> UpdateWorkflow(Guid id, UpsertWorkflowRequest request)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var workflow = await _dbContext.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow is null)
        {
            return NotFound(ApiResponse<WorkflowDetailResponse>.Fail("Workflow not found.", "workflow_not_found"));
        }

        var assigneeValidation = await ValidateAssigneesAsync(request.Steps);

        if (assigneeValidation is not null)
        {
            return assigneeValidation;
        }

        var normalizedName = request.Name.Trim();
        var normalizedStatus = NormalizeStatus(request.Status);
        var duplicateNameExists = await _dbContext.Workflows
            .AnyAsync(existing => existing.Id != id && existing.Name == normalizedName);

        if (duplicateNameExists)
        {
            return Conflict(ApiResponse<WorkflowDetailResponse>.Fail(
                "A workflow with the same name already exists.",
                "workflow_duplicate_name"));
        }

        workflow.Name = normalizedName;
        workflow.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        workflow.Status = normalizedStatus;
        workflow.LastModified = DateTime.UtcNow;

        _dbContext.WorkflowSteps.RemoveRange(workflow.Steps);
        workflow.Steps = MapSteps(request.Steps).ToList();

        await _dbContext.SaveChangesAsync();

        var assigneeLookup = await BuildAssigneeLookupAsync(workflow.Steps);
        var response = MapToDetailResponse(workflow, assigneeLookup);

        return Ok(ApiResponse<WorkflowDetailResponse>.Ok(response, "Workflow updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWorkflow(Guid id)
    {
        var workflow = await _dbContext.Workflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow is null)
        {
            return NotFound(ApiResponse<object>.Fail("Workflow not found.", "workflow_not_found"));
        }

        _dbContext.WorkflowSteps.RemoveRange(workflow.Steps);
        _dbContext.Workflows.Remove(workflow);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Workflow deleted successfully."));
    }

    private ActionResult<ApiResponse<WorkflowDetailResponse>>? ValidateRequest(UpsertWorkflowRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                "Workflow name is required.",
                "workflow_name_required"));
        }

        if (string.IsNullOrWhiteSpace(request.Status) || !AllowedStatuses.ContainsKey(request.Status))
        {
            return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                "Status must be Active, Draft, or Archived.",
                "workflow_invalid_status"));
        }

        if (request.Steps is null || request.Steps.Count == 0)
        {
            return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                "Please provide at least one step for the workflow.",
                "workflow_steps_required"));
        }

        foreach (var step in request.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Name))
            {
                return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                    "Each step must have a name.",
                    "workflow_step_name_required"));
            }

            if (string.IsNullOrWhiteSpace(step.StepType) || !AllowedStepTypes.ContainsKey(step.StepType))
            {
                return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                    "Step type must be Approval, Task, or Notification.",
                    "workflow_invalid_step_type"));
            }

            if (step.Order <= 0)
            {
                return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                    "Step order must be greater than zero.",
                    "workflow_invalid_step_order"));
            }
        }

        return null;
    }

    private async Task<ActionResult<ApiResponse<WorkflowDetailResponse>>?> ValidateAssigneesAsync(
        IEnumerable<WorkflowStepRequest> steps)
    {
        var assigneeIds = steps
            .Select(step => step.AssigneeId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (assigneeIds.Count == 0)
        {
            return null;
        }

        var existingIds = await _userManager.Users
            .AsNoTracking()
            .Where(user => assigneeIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync();

        var missingAssignees = assigneeIds.Except(existingIds).ToList();

        if (missingAssignees.Count > 0)
        {
            return BadRequest(ApiResponse<WorkflowDetailResponse>.Fail(
                "One or more assignees do not exist.",
                "workflow_assignees_missing",
                new Dictionary<string, object?>
                {
                    ["missingAssigneeIds"] = missingAssignees,
                }));
        }

        return null;
    }

    private static IEnumerable<WorkflowStep> MapSteps(IEnumerable<WorkflowStepRequest> steps)
    {
        var orderedSteps = steps
            .OrderBy(step => step.Order)
            .Select((step, index) => new WorkflowStep
            {
                Id = Guid.NewGuid(),
                Name = step.Name.Trim(),
                StepType = NormalizeStepType(step.StepType),
                AssigneeId = string.IsNullOrWhiteSpace(step.AssigneeId) ? null : step.AssigneeId,
                Order = index + 1,
            });

        return orderedSteps;
    }

    private static WorkflowDetailResponse MapToDetailResponse(Workflow workflow, IReadOnlyDictionary<string, string> assignees)
    {
        var steps = workflow.Steps
            .OrderBy(step => step.Order)
            .Select(step => new WorkflowStepResponse(
                step.Id,
                step.Name,
                step.StepType,
                step.AssigneeId,
                step.AssigneeId != null && assignees.TryGetValue(step.AssigneeId, out var assigneeName)
                    ? assigneeName
                    : null,
                step.Order))
            .ToList();

        return new WorkflowDetailResponse(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status,
            workflow.Steps.Count,
            workflow.LastModified,
            steps);
    }

    private async Task<Dictionary<string, string>> BuildAssigneeLookupAsync(IEnumerable<WorkflowStep> steps)
    {
        var assigneeIds = steps
            .Select(step => step.AssigneeId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (assigneeIds.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(user => assigneeIds.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.ToDictionary(
            user => user.Id,
            user => user.DisplayName ?? user.Email ?? user.UserName ?? string.Empty);
    }

    private static string NormalizeStatus(string status)
    {
        return AllowedStatuses.TryGetValue(status, out var normalized)
            ? normalized
            : status.Trim();
    }

    private static string NormalizeStepType(string stepType)
    {
        return AllowedStepTypes.TryGetValue(stepType, out var normalized)
            ? normalized
            : stepType.Trim();
    }
}
