using System.Collections.Generic;

namespace Api.Models.Suppliers;

public class UpsertSupplierRequest
{
    public string CompanyName { get; init; } = string.Empty;

    public string RegistrationNumber { get; init; } = string.Empty;

    public string PrimaryContactName { get; init; } = string.Empty;

    public string PrimaryContactEmail { get; init; } = string.Empty;

    public string PrimaryContactPhone { get; init; } = string.Empty;

    public List<string> BusinessCategories { get; init; } = new();

    public string CompanyAddress { get; init; } = string.Empty;

    public string? Website { get; init; }

    public int YearEstablished { get; init; }

    public int NumberOfEmployees { get; init; }

    public List<string> UploadedDocuments { get; init; } = new();

    public string Status { get; init; } = string.Empty;

    public bool HasPortalAccess { get; init; }

    public string? PortalUserEmail { get; init; }
}
