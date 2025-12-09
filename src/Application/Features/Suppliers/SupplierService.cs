using Application.DTOs.Common;
using Application.DTOs.Suppliers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Suppliers;

public class SupplierService(ISupplierRepository repository, ISupplierPortalUserService portalUserService)
    : ISupplierService
{
    public async Task<PagedResult<SupplierResponse>> GetSuppliersAsync(SupplierQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var totalCount = await repository.CountAsync(search);
        var suppliers = await repository.GetPagedAsync(search, pageNumber, pageSize);

        var response = suppliers.Select(MapToResponse).ToList();
        return new PagedResult<SupplierResponse>(response, totalCount, pageNumber, pageSize);
    }

    public async Task<Result<SupplierResponse>> CreateSupplierAsync(UpsertSupplierRequest request)
    {
        var validation = ValidateStatus(request.Status);
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

        var categories = SerializeList(request.BusinessCategories);
        var categoryList = ParseList(categories);
        var documents = SerializeList(request.UploadedDocuments);

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

        return Result<SupplierResponse>.Ok(MapToResponse(supplier));
    }

    public async Task<Result<SupplierResponse>> UpdateSupplierAsync(Guid id, UpsertSupplierRequest request)
    {
        var supplier = await repository.GetByIdAsync(id);

        if (supplier is null)
        {
            return Result<SupplierResponse>.Fail("suppliers_not_found", "Supplier not found.");
        }

        var validation = ValidateStatus(request.Status);
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

        var categories = SerializeList(request.BusinessCategories);
        var categoryList = ParseList(categories);
        var documents = SerializeList(request.UploadedDocuments);

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

        return Result<SupplierResponse>.Ok(MapToResponse(supplier));
    }

    private static Result<SupplierResponse>? ValidateStatus(string status)
    {
        if (SupplierStatus.All.Contains(status))
        {
            return null;
        }

        return Result<SupplierResponse>.Fail("suppliers_invalid_status", "Invalid status provided.");
    }

    private static string SerializeList(IEnumerable<string> values)
    {
        var sanitized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());

        return string.Join(';', sanitized);
    }

    private static List<string> ParseList(string data)
    {
        return string.IsNullOrWhiteSpace(data)
            ? new List<string>()
            : data.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();
    }

    private static string GenerateSupplierCode()
    {
        var random = Random.Shared.Next(1000, 9999);
        return $"#SUB-{random}";
    }

    private static SupplierResponse MapToResponse(Supplier supplier)
    {
        return new SupplierResponse
        {
            Id = supplier.Id,
            SupplierCode = supplier.SupplierCode,
            CompanyName = supplier.CompanyName,
            RegistrationNumber = supplier.RegistrationNumber,
            PrimaryContactName = supplier.PrimaryContactName,
            PrimaryContactEmail = supplier.PrimaryContactEmail,
            PrimaryContactPhone = supplier.PrimaryContactPhone,
            BusinessCategories = ParseList(supplier.BusinessCategories),
            CompanyAddress = supplier.CompanyAddress,
            Website = supplier.Website,
            YearEstablished = supplier.YearEstablished,
            NumberOfEmployees = supplier.NumberOfEmployees,
            UploadedDocuments = ParseList(supplier.UploadedDocuments),
            Category = supplier.Category,
            ContactPerson = supplier.ContactPerson,
            SubmissionDate = supplier.SubmissionDate.ToString("MM/dd/yyyy"),
            Status = supplier.Status,
            HasPortalAccess = supplier.HasPortalAccess,
            PortalUserEmail = supplier.PortalUserEmail,
        };
    }
}
