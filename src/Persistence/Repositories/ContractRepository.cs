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

    public async Task<bool> ExistsForBidAsync(Guid? bidId)
    {
        if (bidId is null)
        {
            return false;
        }

        return await _dbContext.Contracts.AnyAsync(c => c.BidId == bidId);
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = BuildContractsQuery(search, null);
        return await query.CountAsync();
    }

    public async Task<int> CountForSupplierAsync(string supplierUserId, string? search)
    {
        var query = BuildContractsQuery(search, supplierUserId);
        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<ContractWithReference>> GetContractsAsync(
        string? search,
        int pageNumber,
        int pageSize)
    {
        var query = BuildContractsQuery(search, null);

        return await query
            .OrderByDescending(entry => entry.Contract.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ContractWithReference>> GetContractsForSupplierAsync(
        string supplierUserId,
        string? search,
        int pageNumber,
        int pageSize)
    {
        var query = BuildContractsQuery(search, supplierUserId);

        return await query
            .OrderByDescending(entry => entry.Contract.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ContractWithReference?> GetByIdAsync(Guid contractId)
    {
        return await _dbContext.Contracts
            .Where(contract => contract.Id == contractId)
            .GroupJoin(
                _dbContext.Rfxes,
                contract => contract.RfxId,
                rfx => rfx.Id,
                (contract, rfxGroup) => new { contract, rfxGroup })
            .SelectMany(
                entry => entry.rfxGroup.DefaultIfEmpty(),
                (entry, rfx) => new ContractWithReference
                {
                    Contract = entry.contract,
                    ReferenceNumber = rfx == null ? string.Empty : rfx.ReferenceNumber ?? string.Empty,
                })
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<ContractWithReference> BuildContractsQuery(string? search, string? supplierUserId)
    {
        var query = _dbContext.Contracts
            .GroupJoin(
                _dbContext.Rfxes,
                contract => contract.RfxId,
                rfx => rfx.Id,
                (contract, rfxGroup) => new { contract, rfxGroup })
            .SelectMany(
                entry => entry.rfxGroup.DefaultIfEmpty(),
                (entry, rfx) => new ContractWithReference
                {
                    Contract = entry.contract,
                    ReferenceNumber = rfx == null ? string.Empty : rfx.ReferenceNumber ?? string.Empty,
                })
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(supplierUserId))
        {
            query = query.Where(entry => entry.Contract.SupplierUserId == supplierUserId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();

            query = query.Where(entry =>
                (entry.Contract.Title ?? string.Empty).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                (entry.Contract.SupplierName ?? string.Empty).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                (entry.ReferenceNumber ?? string.Empty).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
