using System;
using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.ContractManagement.Queries;

public class GetSupplierContractByIdQuery
{
    private readonly IContractRepository _contractRepository;
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierContractByIdQuery(
        IContractRepository contractRepository,
        ISupplierRepository supplierRepository)
    {
        _contractRepository = contractRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<ContractDetailResponse>> HandleAsync(Guid contractId, string supplierUserId)
    {
        var contractEntry = await _contractRepository.GetByIdAsync(contractId);
        if (contractEntry is null)
        {
            return Result<ContractDetailResponse>.Fail("not_found", "The requested contract could not be found.");
        }

        if (!string.Equals(contractEntry.Contract.SupplierUserId, supplierUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ContractDetailResponse>.Fail("forbidden", "You are not allowed to view this contract.");
        }

        var supplier = await _supplierRepository.GetByPortalUserIdAsync(contractEntry.Contract.SupplierUserId);

        var supplierDetails = new ContractSupplierDetails(
            supplier?.CompanyName ?? contractEntry.Contract.SupplierName,
            supplier?.PrimaryContactName ?? contractEntry.Contract.SupplierName,
            supplier?.PrimaryContactEmail ?? string.Empty,
            supplier?.PrimaryContactPhone ?? string.Empty,
            supplier?.CompanyAddress ?? string.Empty,
            contractEntry.Contract.SupplierName,
            contractEntry.Contract.SupplierUserId);

        var issuerDetails = new ContractCompanyDetails(
            "Qatar Charity Procurement",
            "Doha, Qatar",
            "procurement@qcharity.org",
            "+974 0000 0000");

        var contract = contractEntry.Contract;
        var response = new ContractDetailResponse(
            contract.Id,
            contract.BidId,
            contract.RfxId,
            contractEntry.ReferenceNumber,
            contract.Title,
            contract.ContractValue,
            contract.Currency,
            contract.StartDateUtc,
            contract.EndDateUtc,
            contract.Status,
            contract.CreatedAtUtc,
            contract.SupplierSignature,
            contract.SupplierSignedAtUtc,
            supplierDetails,
            issuerDetails);

        return Result<ContractDetailResponse>.Ok(response);
    }
}
