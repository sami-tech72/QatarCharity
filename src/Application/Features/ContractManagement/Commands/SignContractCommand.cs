using System;
using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Application.Models;

namespace Application.Features.ContractManagement.Commands;

public class SignContractCommand
{
    private const string DraftStatus = "Draft";
    private const string ActiveStatus = "Active";

    private readonly IContractRepository _contractRepository;

    public SignContractCommand(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<ContractResponse>> HandleAsync(Guid contractId, string supplierUserId, SignContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(supplierUserId))
        {
            return Result<ContractResponse>.Fail("unauthorized", "Missing supplier identity.");
        }

        if (string.IsNullOrWhiteSpace(request.Signature))
        {
            return Result<ContractResponse>.Fail("invalid_signature", "A signature is required to activate the contract.");
        }

        var contractEntry = await _contractRepository.GetByIdAsync(contractId);
        if (contractEntry is null || !string.Equals(contractEntry.Contract.SupplierUserId, supplierUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ContractResponse>.Fail("not_found", "The contract could not be found for this supplier.");
        }

        if (!string.Equals(contractEntry.Contract.Status, DraftStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ContractResponse>.Fail("invalid_status", "Only draft contracts can be signed by the supplier.");
        }

        contractEntry.Contract.SupplierSignature = request.Signature.Trim();
        contractEntry.Contract.SupplierSignedAtUtc = DateTime.UtcNow;
        contractEntry.Contract.Status = ActiveStatus;

        await _contractRepository.SaveChangesAsync();

        var contract = contractEntry.Contract;
        var response = new ContractResponse(
            contract.Id,
            contract.BidId,
            contract.RfxId,
            contractEntry.ReferenceNumber,
            contract.Title,
            contract.SupplierName,
            contract.SupplierUserId,
            contract.ContractValue,
            contract.Currency,
            contract.StartDateUtc,
            contract.EndDateUtc,
            contract.Status,
            contract.CreatedAtUtc,
            contract.SupplierSignature,
            contract.SupplierSignedAtUtc);

        return Result<ContractResponse>.Ok(response);
    }
}
