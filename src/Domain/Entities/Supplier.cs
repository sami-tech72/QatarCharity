using Domain.Enums;

namespace Domain.Entities;

public class Supplier
{
    public Guid Id { get; set; }

    public string SupplierCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string PrimaryContactName { get; set; } = string.Empty;

    public string PrimaryContactEmail { get; set; } = string.Empty;

    public string PrimaryContactPhone { get; set; } = string.Empty;

    public string BusinessCategories { get; set; } = string.Empty;

    public string CompanyAddress { get; set; } = string.Empty;

    public string? Website { get; set; }

    public int YearEstablished { get; set; }

    public int NumberOfEmployees { get; set; }

    public string UploadedDocuments { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = SupplierStatus.Pending;

    public bool HasPortalAccess { get; set; }

    public string? PortalUserEmail { get; set; }

    public string? PortalUserId { get; set; }
}
