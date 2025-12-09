using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Models;

namespace Application.Interfaces.Services;

public interface IRfxService
{
    Task<Result<PagedResult<SupplierBidResponse>>> GetSupplierBidsAsync(SupplierBidQueryParameters query);
    Task<Result<PagedResult<RfxSummaryResponse>>> GetRfxListAsync(string currentUserId, RfxQueryParameters query);
    Task<Result<RfxDetailResponse>> CreateRfxAsync(CreateRfxRequest request);
    Task<Result<RfxDetailResponse>> GetRfxByIdAsync(Guid id, string currentUserId);
    Task<Result<SupplierBidResponse>> EvaluateBidAsync(Guid rfxId, Guid bidId, EvaluateBidRequest request, string currentUserId);
    Task<Result<RfxDetailResponse>> ApproveRfxAsync(Guid id, string currentUserId);
    Task<Result<RfxDetailResponse>> CloseRfxAsync(Guid id, string currentUserId, bool isAdmin);
}
