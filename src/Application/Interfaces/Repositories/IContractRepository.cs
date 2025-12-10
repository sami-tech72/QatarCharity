using System;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IContractRepository
{
    Task AddAsync(Contract contract);
    Task<bool> ExistsForBidAsync(Guid bidId);
    Task<int> CountAsync(string? search);
    Task<IReadOnlyList<(Contract Contract, string ReferenceNumber)>> GetContractsAsync(string? search, int pageNumber, int pageSize);
    Task SaveChangesAsync();
}
