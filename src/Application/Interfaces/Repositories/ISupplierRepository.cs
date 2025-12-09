using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<int> CountAsync(string? search);

    Task<IReadOnlyList<Supplier>> GetPagedAsync(string? search, int pageNumber, int pageSize);

    Task<Supplier?> GetByIdAsync(Guid id);

    Task<bool> PortalEmailInUseAsync(string normalizedEmail, Guid? excludeId = null);

    Task<bool> PortalUserInUseAsync(string portalUserId, Guid? excludeId = null);

    Task AddAsync(Supplier supplier);

    Task SaveChangesAsync();
}
