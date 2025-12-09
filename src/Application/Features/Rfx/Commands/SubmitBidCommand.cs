using System;
using System.Text.Json;
using Application.DTOs.Rfx;
using Application.Features.Rfx.Common;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Rfx.Commands;

public class SubmitBidCommand(IRfxRepository repository)
{
    public async Task<Result<string>> HandleAsync(Guid rfxId, string bidderId, SubmitBidRequest request)
    {
        var validationResult = RfxValidation.ValidateBidSubmission(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var rfx = await repository.GetRfxByIdAsync(rfxId);
        if (rfx is null)
        {
            return Result<string>.Fail("rfx_not_found", "Tender not found.");
        }

        if (!string.Equals(rfx.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Fail("rfx_not_published", "This tender is not open for bids.");
        }

        var documents = request.Documents ?? Array.Empty<BidDocumentSubmission>();
        var inputs = request.Inputs ?? Array.Empty<BidInputSubmission>();

        var requiredDocuments = RfxMapping.DeserializeList(rfx.RequiredDocuments);
        if (requiredDocuments.Any())
        {
            var missingDocs = requiredDocuments
                .Where(doc => !documents.Any(submission =>
                    string.Equals(submission.Name, doc, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(submission.FileName) &&
                    !string.IsNullOrWhiteSpace(submission.ContentBase64)))
                .ToList();

            if (missingDocs.Any())
            {
                return Result<string>.Fail("documents_incomplete", $"Missing required document details: {string.Join(", ", missingDocs)}.");
            }

            var invalidDocument = documents.FirstOrDefault(doc => !RfxValidation.IsBase64(doc.ContentBase64));
            if (invalidDocument is not null)
            {
                return Result<string>.Fail(
                    "documents_invalid",
                    $"The uploaded document for '{invalidDocument.Name}' is invalid or unreadable.");
            }
        }

        var requiredInputs = RfxMapping.BuildRequiredInputs(rfx);
        if (requiredInputs.Any())
        {
            var missingInputs = requiredInputs
                .Where(input => !inputs.Any(submission =>
                    string.Equals(submission.Name, input, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(submission.Value)))
                .ToList();

            if (missingInputs.Any())
            {
                return Result<string>.Fail("inputs_incomplete", $"Missing required input data: {string.Join(", ", missingInputs)}.");
            }
        }

        var bid = new SupplierBid
        {
            RfxId = rfx.Id,
            SubmittedByUserId = bidderId,
            BidAmount = request.BidAmount,
            Currency = request.Currency,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            ProposalSummary = request.ProposalSummary,
            Notes = request.Notes,
            DocumentsJson = JsonSerializer.Serialize(documents),
            InputsJson = JsonSerializer.Serialize(inputs),
            SubmittedAtUtc = DateTime.UtcNow,
        };

        await repository.AddSupplierBidAsync(bid);
        await repository.SaveChangesAsync();

        return Result<string>.Ok("Bid submitted successfully.");
    }
}
