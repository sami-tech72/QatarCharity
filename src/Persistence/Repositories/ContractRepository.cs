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

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
