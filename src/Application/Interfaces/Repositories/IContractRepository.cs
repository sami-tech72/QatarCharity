using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IContractRepository
{
    Task AddAsync(Contract contract);
    Task<bool> ExistsForBidAsync(Guid bidId);
    Task SaveChangesAsync();
}
