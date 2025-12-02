using System;

namespace Api.Models.Suppliers;

public class SupplierResponse
{
    public Guid Id { get; init; }

    public string SupplierCode { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string RegistrationNumber { get; init; } = string.Empty;

    public string PrimaryContactName { get; init; } = string.Empty;

    public string PrimaryContactEmail { get; init; } = string.Empty;

    public string PrimaryContactPhone { get; init; } = string.Empty;

    public IReadOnlyList<string> BusinessCategories { get; init; } = Array.Empty<string>();

    public string CompanyAddress { get; init; } = string.Empty;

    public string? Website { get; init; }

    public int YearEstablished { get; init; }

    public int NumberOfEmployees { get; init; }

    public IReadOnlyList<string> UploadedDocuments { get; init; } = Array.Empty<string>();

    public string Category { get; init; } = string.Empty;

    public string ContactPerson { get; init; } = string.Empty;

    public string SubmissionDate { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public bool HasPortalAccess { get; init; }

    public string? PortalUserEmail { get; init; }
}
