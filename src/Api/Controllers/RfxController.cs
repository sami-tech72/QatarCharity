using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    private static readonly HashSet<string> AllowedBidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending Review",
        "Under Review",
        "Recommended",
        "Approved",
        "Rejected",
        "Needs Clarification",
    };

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public RfxController(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet("bids")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierBidResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierBidResponse>>>> GetSupplierBids([FromQuery] SupplierBidQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var bidsQuery = _dbContext.SupplierBids
            .AsNoTracking()
            .Join(_dbContext.Rfxes.AsNoTracking(), bid => bid.RfxId, rfx => rfx.Id, (bid, rfx) => new
            {
                Bid = bid,
                Rfx = rfx,
            });

        if (!string.IsNullOrWhiteSpace(search))
        {
            bidsQuery = bidsQuery.Where(entry =>
                (entry.Rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(search) ||
                (entry.Rfx.Title ?? string.Empty).ToLower().Contains(search) ||
                (entry.Bid.EvaluationStatus ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await bidsQuery.CountAsync();

        var results = await bidsQuery
            .OrderByDescending(entry => entry.Bid.SubmittedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userLookup = await BuildUserLookupAsync(results
            .Select(entry => entry.Bid.SubmittedByUserId)
            .Concat(results.Select(entry => entry.Bid.EvaluatedByUserId ?? string.Empty))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct());

        var bidResponses = results
            .Select(entry => BuildBidResponse(entry.Bid, entry.Rfx, userLookup))
            .ToList();

        var pagedResult = new PagedResult<SupplierBidResponse>(bidResponses, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<SupplierBidResponse>>.Ok(pagedResult, "Supplier bids retrieved successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RfxSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RfxSummaryResponse>>>> GetRfxList([FromQuery] RfxQueryParameters query)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<PagedResult<RfxSummaryResponse>>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var rfxQuery = _dbContext.Rfxes
            .AsNoTracking()
            .Include(rfx => rfx.Workflow)
            .Include(rfx => rfx.CommitteeMembers)
            .AsQueryable();

        if (query.AssignedOnly)
        {
            rfxQuery = rfxQuery.Where(rfx => rfx.CommitteeMembers.Any(member => member.UserId == currentUserId));
        }

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
                ApprovedCount = rfx.CommitteeMembers.Count(member => member.IsApproved),
                CanApprove = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId),
            })
            .ToListAsync();

        var summaries = rfxes
            .Select(entry => new RfxSummaryResponse(
                entry.Entity.Id,
                entry.Entity.ReferenceNumber,
                entry.Entity.Title,
                entry.Entity.Category,
                entry.Entity.Status,
                entry.Entity.Status.Equals("Published", StringComparison.OrdinalIgnoreCase)
                    ? "Approved"
                    : entry.CommitteeCount > 0
                        ? $"{entry.ApprovedCount}/{entry.CommitteeCount} Approved"
                        : "Pending",
                entry.Entity.ClosingDate,
                entry.Entity.EstimatedBudget,
                entry.Entity.Currency,
                entry.Entity.Workflow?.Name,
                entry.CanApprove))
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> GetRfxById(Guid id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<RfxDetailResponse>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        if (id == Guid.Empty)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "A valid RFx identifier is required.",
                errorCode: "rfx_invalid_id"));
        }

        var rfx = await _dbContext.Rfxes
            .Include(entity => entity.Workflow)
            .Include(entity => entity.EvaluationCriteria)
            .Include(entity => entity.CommitteeMembers)
            .FirstOrDefaultAsync(entity => entity.Id == id);

        if (rfx is null)
        {
            return NotFound(ApiResponse<RfxDetailResponse>.Fail("RFx not found.", errorCode: "rfx_not_found"));
        }

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);

        return Ok(ApiResponse<RfxDetailResponse>.Ok(response, "RFx details retrieved successfully."));
    }

    [HttpPost("{rfxId:guid}/bids/{bidId:guid}/evaluate")]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SupplierBidResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierBidResponse>>> EvaluateBid(Guid rfxId, Guid bidId, [FromBody] EvaluateBidRequest request)
    {
        if (request is null)
        {
            return BadRequest(ApiResponse<SupplierBidResponse>.Fail("Review details are required.", "invalid_request"));
        }

        var normalizedStatus = AllowedBidStatuses.FirstOrDefault(status => status.Equals(request.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedStatus is null)
        {
            return BadRequest(ApiResponse<SupplierBidResponse>.Fail("Invalid bid status provided.", "invalid_status"));
        }

        var rfx = await _dbContext.Rfxes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rfxId);
        if (rfx is null)
        {
            return NotFound(ApiResponse<SupplierBidResponse>.Fail("Tender not found.", "rfx_not_found"));
        }

        var bid = await _dbContext.SupplierBids.FirstOrDefaultAsync(b => b.Id == bidId && b.RfxId == rfxId);
        if (bid is null)
        {
            return NotFound(ApiResponse<SupplierBidResponse>.Fail("Bid not found for this tender.", "bid_not_found"));
        }

        bid.EvaluationStatus = normalizedStatus;
        bid.EvaluationNotes = string.IsNullOrWhiteSpace(request.ReviewNotes) ? null : request.ReviewNotes.Trim();
        bid.EvaluatedAtUtc = DateTime.UtcNow;
        bid.EvaluatedByUserId = GetCurrentUserId();

        await _dbContext.SaveChangesAsync();

        var userLookup = await BuildUserLookupAsync(new[] { bid.SubmittedByUserId, bid.EvaluatedByUserId ?? string.Empty });
        var response = BuildBidResponse(bid, rfx, userLookup);

        return Ok(ApiResponse<SupplierBidResponse>.Ok(response, "Bid evaluation saved."));
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> ApproveRfx(Guid id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<RfxDetailResponse>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var rfx = await _dbContext.Rfxes
            .Include(entity => entity.CommitteeMembers)
            .Include(entity => entity.Workflow)
            .FirstOrDefaultAsync(entity => entity.Id == id);

        if (rfx is null)
        {
            return NotFound(ApiResponse<RfxDetailResponse>.Fail("RFx not found.", errorCode: "rfx_not_found"));
        }

        var isAssignee = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId);

        if (!isAssignee)
        {
            return Forbid();
        }

        if (!rfx.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "Only RFx records in Draft status can be approved.",
                errorCode: "rfx_invalid_status"));
        }

        var committeeMember = rfx.CommitteeMembers.FirstOrDefault(member => member.UserId == currentUserId);

        if (committeeMember is null)
        {
            return Forbid();
        }

        if (committeeMember.IsApproved)
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "You have already approved this RFx.",
                errorCode: "rfx_already_approved"));
        }

        committeeMember.IsApproved = true;
        rfx.LastModified = DateTime.UtcNow;

        if (rfx.CommitteeMembers.All(member => member.IsApproved))
        {
            rfx.Status = "Published";
        }

        await _dbContext.SaveChangesAsync();

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);

        return Ok(ApiResponse<RfxDetailResponse>.Ok(response, "RFx approved and published."));
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RfxDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RfxDetailResponse>>> CloseRfx(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole(Roles.Admin);

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<RfxDetailResponse>.Fail(
                "Invalid or expired token.",
                errorCode: "auth_invalid_token"));
        }

        var rfx = await _dbContext.Rfxes
            .Include(entity => entity.CommitteeMembers)
            .Include(entity => entity.Workflow)
            .FirstOrDefaultAsync(entity => entity.Id == id);

        if (rfx is null)
        {
            return NotFound(ApiResponse<RfxDetailResponse>.Fail("RFx not found.", errorCode: "rfx_not_found"));
        }

        var isAssignee = rfx.CommitteeMembers.Any(member => member.UserId == currentUserId);

        if (!isAdmin && !isAssignee)
        {
            return Forbid();
        }

        if (rfx.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<RfxDetailResponse>.Fail(
                "This RFx is already closed.",
                errorCode: "rfx_already_closed"));
        }

        rfx.Status = "Closed";
        rfx.LastModified = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var response = MapToDetailResponse(rfx, rfx.Workflow?.Name);

        return Ok(ApiResponse<RfxDetailResponse>.Ok(response, "RFx has been closed."));
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

    private static SupplierBidResponse BuildBidResponse(SupplierBid bid, Rfx rfx, IReadOnlyDictionary<string, string> userLookup)
    {
        userLookup.TryGetValue(bid.SubmittedByUserId, out var submittedBy);
        userLookup.TryGetValue(bid.EvaluatedByUserId ?? string.Empty, out var evaluatedBy);

        return new SupplierBidResponse(
            bid.Id,
            bid.RfxId,
            rfx.ReferenceNumber,
            rfx.Title,
            submittedBy ?? bid.SubmittedByUserId,
            bid.BidAmount,
            bid.Currency,
            bid.ExpectedDeliveryDate,
            bid.ProposalSummary,
            bid.Notes,
            bid.SubmittedAtUtc,
            bid.EvaluationStatus,
            bid.EvaluationNotes,
            bid.EvaluatedAtUtc,
            evaluatedBy);
    }

    private async Task<Dictionary<string, string>> BuildUserLookupAsync(IEnumerable<string> userIds)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.ToDictionary(
            user => user.Id,
            user => user.DisplayName ?? user.Email ?? user.UserName ?? user.Id);
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
            .Select(member => new RfxCommitteeMemberResponse(member.Id, member.DisplayName, member.UserId, member.IsApproved))
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

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
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
