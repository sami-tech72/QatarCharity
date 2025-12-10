using Domain.Entities;

namespace Application.DTOs.Contracts;

public class ContractWithReference
{
    public Contract Contract { get; init; } = null!;

    public string ReferenceNumber { get; init; } = string.Empty;
}
