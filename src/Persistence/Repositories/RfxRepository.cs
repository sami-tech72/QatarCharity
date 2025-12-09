using System;
using System.Linq;
using Application.Interfaces.Repositories;
using Domain.Entities;
using RfxEntity = Domain.Entities.Rfx;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class RfxRepository(AppDbContext dbContext) : IRfxRepository
{
    public async Task<int> CountSupplierBidsAsync(string? search)
    {
        var bidsQuery = BuildBidQuery(search);
        return await bidsQuery.CountAsync();
    }

    public async Task<IReadOnlyList<(SupplierBid Bid, RfxEntity Rfx)>> GetSupplierBidsAsync(string? search, int pageNumber, int pageSize)
    {
        var bidsQuery = BuildBidQuery(search);

        var results = await bidsQuery
            .OrderByDescending(entry => entry.Bid.SubmittedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return results.Select(entry => (entry.Bid, entry.Rfx)).ToList();
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
            rfxQuery = rfxQuery.Where(rfx =>
                rfx.ReferenceNumber.ToLower().Contains(search) ||
                rfx.Title.ToLower().Contains(search) ||
                rfx.Category.ToLower().Contains(search));
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
            rfxQuery = rfxQuery.Where(rfx =>
                rfx.ReferenceNumber.ToLower().Contains(search) ||
                rfx.Title.ToLower().Contains(search) ||
                rfx.Category.ToLower().Contains(search));
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
                rfx.Id == rfxId && rfx.Status != null && rfx.Status.ToLower() == "published");
    }

    public async Task AddSupplierBidAsync(SupplierBid bid)
    {
        dbContext.SupplierBids.Add(bid);
        await Task.CompletedTask;
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

    private IQueryable<(SupplierBid Bid, RfxEntity Rfx)> BuildBidQuery(string? search)
    {
        var bidsQuery = dbContext.SupplierBids
            .AsNoTracking()
            .Join(dbContext.Rfxes.AsNoTracking(), bid => bid.RfxId, rfx => rfx.Id, (bid, rfx) => new
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

        return bidsQuery.Select(entry => new ValueTuple<SupplierBid, RfxEntity>(entry.Bid, entry.Rfx));
    }
}
