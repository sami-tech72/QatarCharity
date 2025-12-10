using System;
using Application.DTOs.Common;
using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;

namespace Application.Features.ContractManagement.Queries;

public class GetContractReadyBidsQuery
{
    private readonly IRfxRepository _repository;
    private readonly IUserDirectoryService _userDirectoryService;

    public GetContractReadyBidsQuery(IRfxRepository repository, IUserDirectoryService userDirectoryService)
    {
        _repository = repository;
        _userDirectoryService = userDirectoryService;
    }

    public async Task<Result<PagedResult<ContractReadyBidResponse>>> HandleAsync(ContractReadyQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        const string approvedStatus = "approved";

        var totalCount = await _repository.CountSupplierBidsAsync(search, null, approvedStatus);
        var results = await _repository.GetSupplierBidsAsync(search, null, pageNumber, pageSize, approvedStatus);

        var userIds = results
            .Select(entry => entry.Bid.SubmittedByUserId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var userLookup = await _userDirectoryService.GetUserNamesAsync(userIds);

        var contractReady = results
            .Select(entry =>
            {
                var supplierName = userLookup.TryGetValue(entry.Bid.SubmittedByUserId, out var name)
                    ? name
                    : entry.Bid.SubmittedByUserId;

                return new ContractReadyBidResponse(
                    entry.Bid.Id,
                    entry.Rfx.Id,
                    entry.Rfx.ReferenceNumber,
                    entry.Rfx.Title,
                    entry.Bid.SubmittedByUserId ?? string.Empty,
                    supplierName,
                    entry.Bid.BidAmount,
                    entry.Bid.Currency,
                    entry.Bid.EvaluationStatus,
                    entry.Bid.SubmittedAtUtc,
                    entry.Bid.EvaluatedAtUtc);
            })
            .ToList();

        var pagedResult = new PagedResult<ContractReadyBidResponse>(contractReady, totalCount, pageNumber, pageSize);

        return Result<PagedResult<ContractReadyBidResponse>>.Ok(pagedResult);
    }
}
