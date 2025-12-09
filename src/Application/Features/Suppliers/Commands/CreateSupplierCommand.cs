using Application.DTOs.Suppliers;
using Application.Features.Suppliers.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Suppliers.Commands;

public class CreateSupplierCommand(ISupplierRepository repository, ISupplierPortalUserService portalUserService)
{
    public async Task<Result<SupplierResponse>> HandleAsync(UpsertSupplierRequest request)
    {
        var validation = SupplierMapping.ValidateStatus(request.Status);
        if (validation is not null)
        {
            return validation;
        }

        var portalResult = await portalUserService.EnsurePortalUserAsync(
            request.HasPortalAccess,
            request.PortalUserEmail,
            request.PrimaryContactName,
            null);

        if (!portalResult.Success)
        {
            return Result<SupplierResponse>.Fail(portalResult.ErrorCode!, portalResult.ErrorMessage!);
        }

        var categories = SupplierMapping.SerializeList(request.BusinessCategories);
        var categoryList = SupplierMapping.ParseList(categories);
        var documents = SupplierMapping.SerializeList(request.UploadedDocuments);

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = GenerateSupplierCode(),
            CompanyName = request.CompanyName.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            PrimaryContactName = request.PrimaryContactName.Trim(),
            PrimaryContactEmail = request.PrimaryContactEmail.Trim(),
            PrimaryContactPhone = request.PrimaryContactPhone.Trim(),
            BusinessCategories = categories,
            CompanyAddress = request.CompanyAddress.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            YearEstablished = request.YearEstablished,
            NumberOfEmployees = request.NumberOfEmployees,
            UploadedDocuments = documents,
            Category = categoryList.FirstOrDefault() ?? "General",
            ContactPerson = request.PrimaryContactName.Trim(),
            SubmissionDate = DateTime.UtcNow,
            Status = request.Status,
            HasPortalAccess = request.HasPortalAccess,
            PortalUserEmail = request.HasPortalAccess ? request.PortalUserEmail?.Trim() : null,
            PortalUserId = request.HasPortalAccess ? portalResult.UserId : null,
        };

        await repository.AddAsync(supplier);
        await repository.SaveChangesAsync();

        return Result<SupplierResponse>.Ok(SupplierMapping.ToResponse(supplier));
    }

    private static string GenerateSupplierCode()
    {
        var random = Random.Shared.Next(1000, 9999);
        return $"#SUB-{random}";
    }
}
