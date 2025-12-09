using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;

namespace Application.Features.Suppliers.Commands;

public class UpdateSupplierCommand(ISupplierRepository repository, ISupplierPortalUserService portalUserService)
{
    public async Task<Result<SupplierResponse>> HandleAsync(Guid id, UpsertSupplierRequest request)
    {
        var supplier = await repository.GetByIdAsync(id);

        if (supplier is null)
        {
            return Result<SupplierResponse>.Fail("suppliers_not_found", "Supplier not found.");
        }

        var validation = SupplierMapping.ValidateStatus(request.Status);
        if (validation is not null)
        {
            return validation;
        }

        var portalResult = await portalUserService.EnsurePortalUserAsync(
            request.HasPortalAccess,
            request.PortalUserEmail,
            request.PrimaryContactName,
            id);

        if (!portalResult.Success)
        {
            return Result<SupplierResponse>.Fail(portalResult.ErrorCode!, portalResult.ErrorMessage!);
        }

        var categories = SupplierMapping.SerializeList(request.BusinessCategories);
        var categoryList = SupplierMapping.ParseList(categories);
        var documents = SupplierMapping.SerializeList(request.UploadedDocuments);

        supplier.CompanyName = request.CompanyName.Trim();
        supplier.RegistrationNumber = request.RegistrationNumber.Trim();
        supplier.PrimaryContactName = request.PrimaryContactName.Trim();
        supplier.PrimaryContactEmail = request.PrimaryContactEmail.Trim();
        supplier.PrimaryContactPhone = request.PrimaryContactPhone.Trim();
        supplier.BusinessCategories = categories;
        supplier.CompanyAddress = request.CompanyAddress.Trim();
        supplier.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        supplier.YearEstablished = request.YearEstablished;
        supplier.NumberOfEmployees = request.NumberOfEmployees;
        supplier.UploadedDocuments = documents;
        supplier.Category = categoryList.FirstOrDefault() ?? "General";
        supplier.ContactPerson = request.PrimaryContactName.Trim();
        supplier.Status = request.Status;
        supplier.HasPortalAccess = request.HasPortalAccess;
        supplier.PortalUserEmail = request.HasPortalAccess ? request.PortalUserEmail?.Trim() : null;
        supplier.PortalUserId = request.HasPortalAccess ? portalResult.UserId : null;

        await repository.SaveChangesAsync();

        return Result<SupplierResponse>.Ok(SupplierMapping.ToResponse(supplier));
    }
}
