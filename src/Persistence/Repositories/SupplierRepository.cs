using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class SupplierRepository(AppDbContext dbContext) : ISupplierRepository
{
    public async Task<int> CountAsync(string? search)
    {
        return await BuildSearchQuery(search).CountAsync();
    }

    public async Task<IReadOnlyList<Supplier>> GetPagedAsync(string? search, int pageNumber, int pageSize)
    {
        return await BuildSearchQuery(search)
            .OrderByDescending(supplier => supplier.SubmissionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(Guid id)
    {
        return await dbContext.Suppliers.FindAsync(id);
    }

    public async Task<Supplier?> GetByPortalUserIdAsync(string portalUserId)
    {
        return await dbContext.Suppliers.FirstOrDefaultAsync(supplier => supplier.PortalUserId == portalUserId);
    }

    public async Task<bool> PortalEmailInUseAsync(string normalizedEmail, Guid? excludeId = null)
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(supplier =>
                supplier.HasPortalAccess &&
                supplier.PortalUserEmail != null &&
                supplier.PortalUserEmail.ToLower() == normalizedEmail &&
                (!excludeId.HasValue || supplier.Id != excludeId.Value));
    }

    public async Task<bool> PortalUserInUseAsync(string portalUserId, Guid? excludeId = null)
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(supplier =>
                supplier.HasPortalAccess &&
                supplier.PortalUserId == portalUserId &&
                (!excludeId.HasValue || supplier.Id != excludeId.Value));
    }

    public async Task AddAsync(Supplier supplier)
    {
        await dbContext.Suppliers.AddAsync(supplier);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<Supplier> BuildSearchQuery(string? search)
    {
        var suppliersQuery = dbContext.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            suppliersQuery = suppliersQuery.Where(supplier =>
                supplier.SupplierCode.ToLower().Contains(search) ||
                supplier.CompanyName.ToLower().Contains(search) ||
                supplier.RegistrationNumber.ToLower().Contains(search) ||
                supplier.PrimaryContactName.ToLower().Contains(search) ||
                supplier.PrimaryContactEmail.ToLower().Contains(search) ||
                (supplier.PortalUserEmail ?? string.Empty).ToLower().Contains(search));
        }

        return suppliersQuery;
    }
}
