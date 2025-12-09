using Application.DTOs.Suppliers;
using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Suppliers.Common;

internal static class SupplierMapping
{
    public static Result<SupplierResponse>? ValidateStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status) &&
            SupplierStatus.All.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        return Result<SupplierResponse>.Fail("suppliers_invalid_status", "Invalid status provided.");
    }

    public static string SerializeList(IEnumerable<string> values)
    {
        var sanitized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());

        return string.Join(';', sanitized);
    }

    public static List<string> ParseList(string data)
    {
        return string.IsNullOrWhiteSpace(data)
            ? new List<string>()
            : data.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();
    }

    public static SupplierResponse ToResponse(Supplier supplier)
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
