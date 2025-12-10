using System;
using System.Linq;
using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly AppDbContext _dbContext;

    public ContractRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Contract contract)
    {
        await _dbContext.Contracts.AddAsync(contract);
    }

    public async Task<bool> ExistsForBidAsync(Guid bidId)
    {
        return await _dbContext.Contracts.AnyAsync(c => c.BidId == bidId);
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = BuildContractsQuery(search);
        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<ContractWithReference>> GetContractsAsync(
        string? search,
        int pageNumber,
        int pageSize)
    {
        var query = BuildContractsQuery(search);

        return await query
            .OrderByDescending(entry => entry.Contract.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(entry => new ContractWithReference
            {
                Contract = entry.Contract,
                ReferenceNumber = entry.ReferenceNumber,
            })
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<ContractWithReference> BuildContractsQuery(string? search)
    {
        var query = _dbContext.Contracts
            .Join(
                _dbContext.Rfxes,
                contract => contract.RfxId,
                rfx => rfx.Id,
                (contract, rfx) => new ContractWithReference
                {
                    Contract = contract,
                    ReferenceNumber = rfx.ReferenceNumber ?? string.Empty,
                })
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(entry =>
                (entry.Contract.Title ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (entry.Contract.SupplierName ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                (entry.ReferenceNumber ?? string.Empty).ToLower().Contains(normalizedSearch));
        }

        return query;
    }
}
