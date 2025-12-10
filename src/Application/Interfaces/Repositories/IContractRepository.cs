using System;
using Application.DTOs.Contracts;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IContractRepository
{
    Task AddAsync(Contract contract);
    Task<bool> ExistsForBidAsync(Guid bidId);
    Task<int> CountAsync(string? search);
    Task<int> CountForSupplierAsync(string supplierUserId, string? search);
    Task<IReadOnlyList<ContractWithReference>> GetContractsAsync(string? search, int pageNumber, int pageSize);
    Task<IReadOnlyList<ContractWithReference>> GetContractsForSupplierAsync(
        string supplierUserId,
        string? search,
        int pageNumber,
        int pageSize);
    Task<ContractWithReference?> GetByIdAsync(Guid contractId);
    Task SaveChangesAsync();
}
