using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Domain.Entities;
using RfxEntity = Domain.Entities.Rfx;

namespace Application.Interfaces.Repositories;

public interface IRfxRepository
{
    Task<int> CountSupplierBidsAsync(string? search, string? submittedByUserId = null, string? evaluationStatus = null);
    Task<IReadOnlyList<(SupplierBid Bid, RfxEntity Rfx)>> GetSupplierBidsAsync(string? search, string? submittedByUserId, int pageNumber, int pageSize, string? evaluationStatus = null);
    Task<IReadOnlyList<RfxEntity>> GetRfxSummariesAsync(string? search, bool assignedOnly, string currentUserId, int pageNumber, int pageSize);
    Task<int> CountRfxAsync(string? search, bool assignedOnly, string currentUserId);
    Task<RfxEntity?> GetRfxByIdAsync(Guid id);
    Task<Workflow?> GetWorkflowAsync(Guid id);
    Task<RfxEntity?> GetRfxWithEvaluationAsync(Guid id);
    Task AddRfxAsync(RfxEntity rfx);
    Task SaveChangesAsync();
    Task<SupplierBid?> GetBidAsync(Guid rfxId, Guid bidId);
    Task<string> GenerateReferenceNumberAsync();
    Task<List<string>> GetMissingCommitteeMemberIdsAsync(IEnumerable<string> committeeMemberIds);
    Task<List<RfxCommitteeMember>> BuildCommitteeMembersAsync(IEnumerable<string> committeeMemberIds);
    Task<int> CountPublishedRfxAsync(string? search);
    Task<IReadOnlyList<RfxEntity>> GetPublishedRfxAsync(string? search, int pageNumber, int pageSize);
    Task<RfxEntity?> GetPublishedRfxByIdAsync(Guid rfxId);
    Task AddSupplierBidAsync(SupplierBid bid);

    Task AddBidReviewAsync(SupplierBidReview review);
    Task<List<SupplierBidReview>> GetBidReviewsAsync(Guid bidId);
    Task<SupplierBidReview?> GetBidReviewForUserAsync(Guid bidId, string userId);
    Task<Dictionary<Guid, List<SupplierBidReview>>> GetBidReviewsAsync(IEnumerable<Guid> bidIds);
}
