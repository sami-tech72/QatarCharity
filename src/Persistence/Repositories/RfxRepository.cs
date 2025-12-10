using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces.Repositories;
using Domain.Entities;
using RfxEntity = Domain.Entities.Rfx;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class RfxRepository(AppDbContext dbContext) : IRfxRepository
{
    public async Task<int> CountSupplierBidsAsync(
        string? search,
        string? submittedByUserId = null,
        string? evaluationStatus = null,
        bool excludeWithContract = false)
    {
        var bidsQuery = BuildBidQuery(search, submittedByUserId, evaluationStatus, excludeWithContract);
        return await bidsQuery.CountAsync();
    }

    public async Task<IReadOnlyList<(SupplierBid Bid, RfxEntity Rfx)>> GetSupplierBidsAsync(
        string? search,
        string? submittedByUserId,
        int pageNumber,
        int pageSize,
        string? evaluationStatus = null,
        bool excludeWithContract = false)
    {
        var query = BuildBidQuery(search, submittedByUserId, evaluationStatus, excludeWithContract);

        var results = await query
            .OrderByDescending(x => x.Bid.SubmittedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return results
            .Select(entry => (entry.Bid, entry.Rfx))
            .ToList();
    }



    public async Task<IReadOnlyList<RfxEntity>> GetRfxSummariesAsync(string? search, bool assignedOnly, string currentUserId, int pageNumber, int pageSize)
    {
        var rfxQuery = dbContext.Rfxes
            .Include(rfx => rfx.Workflow)
            .Include(rfx => rfx.CommitteeMembers)
            .AsQueryable();

        if (assignedOnly)
        {
            rfxQuery = rfxQuery.Where(rfx => rfx.CommitteeMembers.Any(member => member.UserId == currentUserId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (rfx.Title ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (rfx.Category ?? string.Empty).ToLower().Contains(normalizedSearch));
        }

        return await rfxQuery
            .OrderByDescending(rfx => rfx.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountRfxAsync(string? search, bool assignedOnly, string currentUserId)
    {
        var rfxQuery = dbContext.Rfxes.AsQueryable();

        if (assignedOnly)
        {
            rfxQuery = rfxQuery.Where(rfx => rfx.CommitteeMembers.Any(member => member.UserId == currentUserId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (rfx.Title ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (rfx.Category ?? string.Empty).ToLower().Contains(normalizedSearch));
        }

        return await rfxQuery.CountAsync();
    }

    public Task<RfxEntity?> GetRfxByIdAsync(Guid id)
    {
        return dbContext.Rfxes.AsNoTracking().FirstOrDefaultAsync(rfx => rfx.Id == id);
    }

    public Task<Workflow?> GetWorkflowAsync(Guid id)
    {
        return dbContext.Workflows.AsNoTracking().FirstOrDefaultAsync(wf => wf.Id == id);
    }

    public Task<RfxEntity?> GetRfxWithEvaluationAsync(Guid id)
    {
        return dbContext.Rfxes
            .Include(entity => entity.Workflow)
            .Include(entity => entity.EvaluationCriteria)
            .Include(entity => entity.CommitteeMembers)
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public async Task AddRfxAsync(RfxEntity rfx)
    {
        await dbContext.Rfxes.AddAsync(rfx);
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }

    public Task<SupplierBid?> GetBidAsync(Guid rfxId, Guid bidId)
    {
        return dbContext.SupplierBids.FirstOrDefaultAsync(b => b.Id == bidId && b.RfxId == rfxId);
    }

    public async Task<string> GenerateReferenceNumberAsync()
    {
        var now = DateTime.UtcNow;
        var count = await dbContext.Rfxes.CountAsync(rfx => rfx.CreatedAt.Year == now.Year);
        return $"RFx-{now:yyyy}-{count + 1:D4}";
    }

    public async Task<List<string>> GetMissingCommitteeMemberIdsAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new List<string>();
        }

        var existingIds = await dbContext.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync();

        return ids.Except(existingIds).ToList();
    }

    public async Task<List<RfxCommitteeMember>> BuildCommitteeMembersAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new List<RfxCommitteeMember>();
        }

        var users = await dbContext.Users
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

    public async Task<int> CountPublishedRfxAsync(string? search)
    {
        var query = BuildPublishedRfxQuery(search);
        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<RfxEntity>> GetPublishedRfxAsync(string? search, int pageNumber, int pageSize)
    {
        var query = BuildPublishedRfxQuery(search);

        return await query
            .OrderBy(rfx => rfx.SubmissionDeadline)
            .ThenBy(rfx => rfx.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RfxEntity?> GetPublishedRfxByIdAsync(Guid rfxId)
    {
        return await dbContext.Rfxes
            .AsNoTracking()
            .FirstOrDefaultAsync(rfx =>
                rfx.Id == rfxId && (rfx.Status ?? string.Empty).ToLower() == "published");
    }

    public async Task AddSupplierBidAsync(SupplierBid bid)
    {
        dbContext.SupplierBids.Add(bid);
        await Task.CompletedTask;
    }


    public Task AddBidReviewAsync(SupplierBidReview review)
    {
        dbContext.SupplierBidReviews.Add(review);
        return Task.CompletedTask;
    }

    public Task<List<SupplierBidReview>> GetBidReviewsAsync(Guid bidId)
    {
        return dbContext.SupplierBidReviews
            .AsNoTracking()
            .Where(r => r.BidId == bidId)
            .OrderByDescending(r => r.ReviewedAtUtc)
            .ToListAsync();
    }

    public Task<SupplierBidReview?> GetBidReviewForUserAsync(Guid bidId, string userId)
    {
        return dbContext.SupplierBidReviews
            .FirstOrDefaultAsync(r => r.BidId == bidId && r.ReviewerUserId == userId);
    }

    public async Task<Dictionary<Guid, List<SupplierBidReview>>> GetBidReviewsAsync(IEnumerable<Guid> bidIds)
    {
        var normalizedIds = bidIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            return new Dictionary<Guid, List<SupplierBidReview>>();
        }

        var reviews = await dbContext.SupplierBidReviews
            .AsNoTracking()
            .Where(r => normalizedIds.Contains(r.BidId))
            .OrderByDescending(r => r.ReviewedAtUtc)
            .ToListAsync();

        return reviews
            .GroupBy(r => r.BidId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private IQueryable<RfxEntity> BuildPublishedRfxQuery(string? search)
    {
        var rfxQuery = dbContext.Rfxes
            .AsNoTracking()
            .Where(rfx => rfx.Status != null && rfx.Status.ToLower() == "published");

        if (!string.IsNullOrWhiteSpace(search))
        {
            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(search) ||
                (rfx.Title ?? string.Empty).ToLower().Contains(search) ||
                (rfx.Category ?? string.Empty).ToLower().Contains(search));
        }

        return rfxQuery;
    }

    private IQueryable<BidWithRfx> BuildBidQuery(string? search, string? submittedByUserId, string? evaluationStatus, bool excludeWithContract)
    {
        var bidsQuery = dbContext.SupplierBids
            .AsNoTracking()
            .Join(dbContext.Rfxes.AsNoTracking(), bid => bid.RfxId, rfx => rfx.Id, (bid, rfx) => new BidWithRfx
            {
                Bid = bid,
                Rfx = rfx,
            });

        if (excludeWithContract)
        {
            bidsQuery = bidsQuery.Where(entry => !dbContext.Contracts.Any(contract => contract.BidId == entry.Bid.Id));
        }

        if (!string.IsNullOrWhiteSpace(submittedByUserId))
        {
            bidsQuery = bidsQuery.Where(entry => entry.Bid.SubmittedByUserId == submittedByUserId);
        }

        if (!string.IsNullOrWhiteSpace(evaluationStatus))
        {
            var normalizedStatus = evaluationStatus.Trim().ToLower();
            bidsQuery = bidsQuery.Where(entry => (entry.Bid.EvaluationStatus ?? string.Empty).ToLower() == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            bidsQuery = bidsQuery.Where(entry =>
                (entry.Rfx.ReferenceNumber ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (entry.Rfx.Title ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (entry.Bid.EvaluationStatus ?? string.Empty).ToLower().Contains(normalizedSearch));
        }

        return bidsQuery;
    }

    private sealed class BidWithRfx
    {
        public required SupplierBid Bid { get; init; }

        public required RfxEntity Rfx { get; init; }
    }
}
