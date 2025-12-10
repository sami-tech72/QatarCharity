using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        return results.Select(entry => (entry.Bid, entry.Rfx)).ToList();
    }

    public async Task<IReadOnlyList<RfxEntity>> GetRfxSummariesAsync(
        string? search, bool assignedOnly, string currentUserId,
        int pageNumber, int pageSize)
    {
        var rfxQuery = dbContext.Rfxes
            .Include(rfx => rfx.Workflow)
            .Include(rfx => rfx.CommitteeMembers)
            .AsQueryable();

        if (assignedOnly)
        {
            rfxQuery = rfxQuery.Where(rfx =>
                rfx.CommitteeMembers.Any(member => member.UserId == currentUserId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();

            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? "").ToLower().Contains(searchLower) ||
                (rfx.Title ?? "").ToLower().Contains(searchLower) ||
                (rfx.Category ?? "").ToLower().Contains(searchLower));
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
            rfxQuery = rfxQuery.Where(rfx =>
                rfx.CommitteeMembers.Any(member => member.UserId == currentUserId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();

            rfxQuery = rfxQuery.Where(rfx =>
                (rfx.ReferenceNumber ?? "").ToLower().Contains(searchLower) ||
                (rfx.Title ?? "").ToLower().Contains(searchLower) ||
                (rfx.Category ?? "").ToLower().Contains(searchLower));
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
            .Include(e => e.Workflow)
            .Include(e => e.EvaluationCriteria)
            .Include(e => e.CommitteeMembers)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddRfxAsync(RfxEntity rfx)
    {
        await dbContext.Rfxes.AddAsync(rfx);
    }

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();

    public Task<SupplierBid?> GetBidAsync(Guid rfxId, Guid bidId)
    {
        return dbContext.SupplierBids
            .FirstOrDefaultAsync(b => b.Id == bidId && b.RfxId == rfxId);
    }

    public async Task<string> GenerateReferenceNumberAsync()
    {
        var now = DateTime.UtcNow;
        var count = await dbContext.Rfxes.CountAsync(r => r.CreatedAt.Year == now.Year);
        return $"RFx-{now:yyyy}-{count + 1:D4}";
    }

    public async Task<List<string>> GetMissingCommitteeMemberIdsAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();

        if (!ids.Any())
            return new List<string>();

        var existingIds = await dbContext.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync();

        return ids.Except(existingIds).ToList();
    }

    public async Task<List<RfxCommitteeMember>> BuildCommitteeMembersAsync(IEnumerable<string> committeeMemberIds)
    {
        var ids = committeeMemberIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();

        if (!ids.Any())
            return new List<RfxCommitteeMember>();

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.Select(user => new RfxCommitteeMember
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DisplayName = user.DisplayName ?? user.Email ?? user.UserName ?? ""
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
            .OrderBy(r => r.SubmissionDeadline)
            .ThenBy(r => r.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RfxEntity?> GetPublishedRfxByIdAsync(Guid rfxId)
    {
        return await dbContext.Rfxes
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Id == rfxId &&
                r.Status.ToLower() == "published");
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
        var ids = bidIds.Where(id => id != Guid.Empty).Distinct().ToList();

        if (!ids.Any())
            return new Dictionary<Guid, List<SupplierBidReview>>();

        var reviews = await dbContext.SupplierBidReviews
            .AsNoTracking()
            .Where(r => ids.Contains(r.BidId))
            .OrderByDescending(r => r.ReviewedAtUtc)
            .ToListAsync();

        return reviews.GroupBy(r => r.BidId)
                      .ToDictionary(g => g.Key, g => g.ToList());
    }

    private IQueryable<RfxEntity> BuildPublishedRfxQuery(string? search)
    {
        var query = dbContext.Rfxes
            .AsNoTracking()
            .Where(r => r.Status != null && r.Status.ToLower() == "published");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();

            query = query.Where(r =>
                (r.ReferenceNumber ?? "").ToLower().Contains(s) ||
                (r.Title ?? "").ToLower().Contains(s) ||
                (r.Category ?? "").ToLower().Contains(s));
        }

        return query;
    }

    private IQueryable<BidWithRfx> BuildBidQuery(string? search, string? submittedByUserId, string? evaluationStatus, bool excludeWithContract)
    {
        var bids = dbContext.SupplierBids
            .AsNoTracking()
            .Join(dbContext.Rfxes.AsNoTracking(),
                bid => bid.RfxId,
                rfx => rfx.Id,
                (bid, rfx) => new BidWithRfx { Bid = bid, Rfx = rfx });

        if (excludeWithContract)
        {
            bids = bids.Where(entry =>
                !dbContext.Contracts.Any(c => c.BidId == entry.Bid.Id));
        }

        if (!string.IsNullOrWhiteSpace(submittedByUserId))
        {
            bids = bids.Where(entry => entry.Bid.SubmittedByUserId == submittedByUserId);
        }

        if (!string.IsNullOrWhiteSpace(evaluationStatus))
        {
            var evalLower = evaluationStatus.Trim().ToLower();

            bids = bids.Where(entry =>
                (entry.Bid.EvaluationStatus ?? "").ToLower() == evalLower);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();

            bids = bids.Where(entry =>
                (entry.Rfx.ReferenceNumber ?? "").ToLower().Contains(s) ||
                (entry.Rfx.Title ?? "").ToLower().Contains(s) ||
                (entry.Bid.EvaluationStatus ?? "").ToLower().Contains(s));
        }

        return bids;
    }

    private sealed class BidWithRfx
    {
        public required SupplierBid Bid { get; init; }
        public required RfxEntity Rfx { get; init; }
    }
}
