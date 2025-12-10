using Application.DTOs.Contracts;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities;

namespace Application.Features.ContractManagement.Commands;

public class CreateContractCommand
{
    private const string DefaultStatus = "Draft";

    private readonly IContractRepository _contractRepository;
    private readonly IRfxRepository _rfxRepository;

    public CreateContractCommand(IContractRepository contractRepository, IRfxRepository rfxRepository)
    {
        _contractRepository = contractRepository;
        _rfxRepository = rfxRepository;
    }

    public async Task<Result<ContractResponse>> HandleAsync(CreateContractRequest request)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return Result<ContractResponse>.Fail("invalid_request", validationError);
        }

        var rfx = await _rfxRepository.GetRfxByIdAsync(request.RfxId);
        if (rfx is null)
        {
            return Result<ContractResponse>.Fail("not_found", "The RFx referenced by this bid could not be found.");
        }

        var bid = await _rfxRepository.GetBidAsync(request.RfxId, request.BidId);
        if (bid is null)
        {
            return Result<ContractResponse>.Fail("not_found", "The selected bid could not be found for this RFx.");
        }

        if (!string.Equals(bid.EvaluationStatus, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ContractResponse>.Fail("invalid_status", "Only approved bids can be converted into contracts.");
        }

        var exists = await _contractRepository.ExistsForBidAsync(request.BidId);
        if (exists)
        {
            return Result<ContractResponse>.Fail("duplicate", "A contract already exists for this bid.");
        }

        var normalizedCurrency = request.Currency.Trim();
        var normalizedSupplierId = request.SupplierUserId.Trim();

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            BidId = request.BidId,
            RfxId = request.RfxId,
            Title = request.Title.Trim(),
            SupplierName = request.SupplierName.Trim(),
            SupplierUserId = normalizedSupplierId,
            ContractValue = request.ContractValue,
            Currency = normalizedCurrency,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            Status = DefaultStatus,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _contractRepository.AddAsync(contract);
        await _contractRepository.SaveChangesAsync();

        var response = new ContractResponse(
            contract.Id,
            contract.BidId,
            contract.RfxId,
            rfx.ReferenceNumber ?? string.Empty,
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

    private static string? ValidateRequest(CreateContractRequest request)
    {
        if (request.BidId == Guid.Empty || request.RfxId == Guid.Empty)
        {
            return "A valid bid and RFx reference are required.";
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "Contract title is required.";
        }

        if (string.IsNullOrWhiteSpace(request.SupplierName) || string.IsNullOrWhiteSpace(request.SupplierUserId))
        {
            return "Supplier details are required.";
        }

        if (request.ContractValue <= 0)
        {
            return "Contract value must be greater than zero.";
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return "Currency is required.";
        }

        if (request.StartDateUtc == default || request.EndDateUtc == default)
        {
            return "Start and end dates are required.";
        }

        if (request.EndDateUtc < request.StartDateUtc)
        {
            return "End date must be after the start date.";
        }

        return null;
    }
}
