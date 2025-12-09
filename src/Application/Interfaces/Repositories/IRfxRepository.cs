using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRfxRepository
{
    Task<int> CountSupplierBidsAsync(string? search);
    Task<IReadOnlyList<(SupplierBid Bid, Rfx Rfx)>> GetSupplierBidsAsync(string? search, int pageNumber, int pageSize);
    Task<IReadOnlyList<Rfx>> GetRfxSummariesAsync(string? search, bool assignedOnly, string currentUserId, int pageNumber, int pageSize);
    Task<int> CountRfxAsync(string? search, bool assignedOnly, string currentUserId);
    Task<Rfx?> GetRfxByIdAsync(Guid id);
    Task<Workflow?> GetWorkflowAsync(Guid id);
    Task<Rfx?> GetRfxWithEvaluationAsync(Guid id);
    Task AddRfxAsync(Rfx rfx);
    Task SaveChangesAsync();
    Task<SupplierBid?> GetBidAsync(Guid rfxId, Guid bidId);
    Task<string> GenerateReferenceNumberAsync();
    Task<List<string>> GetMissingCommitteeMemberIdsAsync(IEnumerable<string> committeeMemberIds);
    Task<List<RfxCommitteeMember>> BuildCommitteeMembersAsync(IEnumerable<string> committeeMemberIds);
    Task<int> CountPublishedRfxAsync(string? search);
    Task<IReadOnlyList<Rfx>> GetPublishedRfxAsync(string? search, int pageNumber, int pageSize);
    Task<Rfx?> GetPublishedRfxByIdAsync(Guid rfxId);
    Task AddSupplierBidAsync(SupplierBid bid);
}
