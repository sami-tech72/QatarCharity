namespace Application.DTOs.Suppliers;

public class SupplierProfileRequest
{
    public string CompanyName { get; init; } = string.Empty;

    public string RegistrationNumber { get; init; } = string.Empty;

    public string PrimaryContactName { get; init; } = string.Empty;

    public string PrimaryContactEmail { get; init; } = string.Empty;

    public string PrimaryContactPhone { get; init; } = string.Empty;

    public string CompanyAddress { get; init; } = string.Empty;

    public string? Website { get; init; }

    public int YearEstablished { get; init; }

    public int NumberOfEmployees { get; init; }
}
