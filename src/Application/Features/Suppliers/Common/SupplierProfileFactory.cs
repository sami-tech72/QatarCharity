using System;
using System.Linq;
using Application.DTOs.Suppliers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Features.Suppliers.Common;

internal static class SupplierProfileFactory
{
    public static Supplier CreateFromCurrentUser(string portalUserId, string? email, string? displayName)
    {
        var resolvedName = ResolveDisplayName(displayName, email);
        var resolvedEmail = email?.Trim() ?? string.Empty;

        return new Supplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = BuildSupplierCode(portalUserId),
            CompanyName = resolvedName,
            RegistrationNumber = string.Empty,
            PrimaryContactName = resolvedName,
            PrimaryContactEmail = resolvedEmail,
            PrimaryContactPhone = string.Empty,
            BusinessCategories = string.Empty,
            CompanyAddress = string.Empty,
            Website = null,
            YearEstablished = DateTime.UtcNow.Year,
            NumberOfEmployees = 0,
            UploadedDocuments = string.Empty,
            Category = string.Empty,
            ContactPerson = resolvedName,
            SubmissionDate = DateTime.UtcNow,
            Status = SupplierStatus.Pending,
            HasPortalAccess = true,
            PortalUserEmail = email?.Trim(),
            PortalUserId = portalUserId,
        };
    }

    public static Supplier CreateFromRequest(string portalUserId, SupplierProfileRequest request)
    {
        return new Supplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = BuildSupplierCode(portalUserId),
            CompanyName = request.CompanyName.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            PrimaryContactName = request.PrimaryContactName.Trim(),
            PrimaryContactEmail = request.PrimaryContactEmail.Trim(),
            PrimaryContactPhone = request.PrimaryContactPhone.Trim(),
            BusinessCategories = string.Empty,
            CompanyAddress = request.CompanyAddress.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            YearEstablished = request.YearEstablished,
            NumberOfEmployees = request.NumberOfEmployees,
            UploadedDocuments = string.Empty,
            Category = string.Empty,
            ContactPerson = request.PrimaryContactName.Trim(),
            SubmissionDate = DateTime.UtcNow,
            Status = SupplierStatus.Pending,
            HasPortalAccess = true,
            PortalUserEmail = request.PrimaryContactEmail.Trim(),
            PortalUserId = portalUserId,
        };
    }

    private static string ResolveDisplayName(string? displayName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        return string.IsNullOrWhiteSpace(email) ? "Supplier" : email.Trim();
    }

    private static string BuildSupplierCode(string portalUserId)
    {
        var suffix = string.IsNullOrWhiteSpace(portalUserId)
            ? Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()
            : new string(portalUserId
                .Where(char.IsLetterOrDigit)
                .Reverse()
                .Take(8)
                .Reverse()
                .ToArray())
                .PadRight(6, 'X')
                .ToUpperInvariant();

        return $"#SUB-{suffix}";
    }
}
