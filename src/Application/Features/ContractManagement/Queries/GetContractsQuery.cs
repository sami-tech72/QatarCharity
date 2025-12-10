using System;
using System.Linq;
using Application.DTOs.Common;
using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.ContractManagement.Queries;

public class GetContractsQuery
{
    private readonly IContractRepository _contractRepository;

    public GetContractsQuery(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<PagedResult<ContractResponse>>> HandleAsync(ContractReadyQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim();

        var totalCount = await _contractRepository.CountAsync(search);
        var contracts = await _contractRepository.GetContractsAsync(search, pageNumber, pageSize);

        var results = contracts
            .Select(entry =>
                new ContractResponse(
                    entry.Contract.Id,
                    entry.Contract.BidId,
                    entry.Contract.RfxId,
                    entry.ReferenceNumber,
                    entry.Contract.Title,
                    entry.Contract.SupplierName,
                    entry.Contract.SupplierUserId,
                    entry.Contract.ContractValue,
                    entry.Contract.Currency,
                    entry.Contract.StartDateUtc,
                    entry.Contract.EndDateUtc,
                    entry.Contract.Status,
                    entry.Contract.CreatedAtUtc))
            .ToList();

        var paged = new PagedResult<ContractResponse>(results, totalCount, pageNumber, pageSize);
        return Result<PagedResult<ContractResponse>>.Ok(paged);
    }
}
