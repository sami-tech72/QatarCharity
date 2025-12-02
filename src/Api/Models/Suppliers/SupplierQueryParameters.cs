namespace Api.Models.Suppliers;

public class SupplierQueryParameters
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
