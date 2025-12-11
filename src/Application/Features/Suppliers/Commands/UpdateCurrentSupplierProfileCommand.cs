using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.Suppliers.Commands;

public class UpdateCurrentSupplierProfileCommand(ISupplierRepository repository)
{
    public async Task<Result<SupplierResponse>> HandleAsync(string portalUserId, SupplierProfileRequest request)
    {
        var supplier = await repository.GetByPortalUserIdAsync(portalUserId);

        if (supplier is null)
        {
            return Result<SupplierResponse>.Fail("suppliers_not_found", "Supplier not found.");
        }

        supplier.CompanyName = request.CompanyName.Trim();
        supplier.RegistrationNumber = request.RegistrationNumber.Trim();
        supplier.PrimaryContactName = request.PrimaryContactName.Trim();
        supplier.PrimaryContactEmail = request.PrimaryContactEmail.Trim();
        supplier.PrimaryContactPhone = request.PrimaryContactPhone.Trim();
        supplier.CompanyAddress = request.CompanyAddress.Trim();
        supplier.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        supplier.YearEstablished = request.YearEstablished;
        supplier.NumberOfEmployees = request.NumberOfEmployees;
        supplier.ContactPerson = request.PrimaryContactName.Trim();

        await repository.SaveChangesAsync();

        return Result<SupplierResponse>.Ok(SupplierMapping.ToResponse(supplier));
    }
}
