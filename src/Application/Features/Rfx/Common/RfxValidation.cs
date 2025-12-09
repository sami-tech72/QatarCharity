using System;
using Application.DTOs.Common;
using Application.DTOs.Rfx;
using Application.Models;

namespace Application.Features.Rfx.Common;

internal static class RfxValidation
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Published",
        "Closed",
    };

    private static readonly HashSet<string> AllowedBidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending Review",
        "Under Review",
        "Recommended",
        "Approved",
        "Rejected",
        "Needs Clarification",
    };

    public static bool IsValidStatus(string status) => AllowedStatuses.Contains(NormalizeStatus(status));

    public static string NormalizeStatus(string status)
    {
        return AllowedStatuses.FirstOrDefault(state => state.Equals(status?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? "Draft";
    }

    public static Result<RfxDetailResponse>? ValidateCreateRequest(CreateRfxRequest request, IReadOnlyCollection<string> missingCommitteeMembers)
    {
        if (request is null)
        {
            return Result<RfxDetailResponse>.Fail("invalid_request", "A valid RFx payload is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_title", "A title is required.");
        }

        if (missingCommitteeMembers.Count > 0)
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_committee", "One or more committee members are invalid.");
        }

        var normalizedStatus = NormalizeStatus(request.Status);
        if (!AllowedStatuses.Contains(normalizedStatus))
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_status", "Invalid RFx status provided.");
        }

        if (request.ClosingDate <= DateTime.UtcNow)
        {
            return Result<RfxDetailResponse>.Fail("rfx_invalid_dates", "Closing date must be in the future.");
        }

        return null;
    }

    public static Result<SupplierBidResponse>? ValidateBidStatus(EvaluateBidRequest request)
    {
        if (request is null)
        {
            return Result<SupplierBidResponse>.Fail("invalid_request", "Review details are required.");
        }

        var normalizedStatus = NormalizeBidStatus(request.Status);
        if (normalizedStatus is null)
        {
            return Result<SupplierBidResponse>.Fail("invalid_status", "Invalid bid status provided.");
        }

        return null;
    }

    public static string? NormalizeBidStatus(string? status)
    {
        return AllowedBidStatuses.FirstOrDefault(value => value.Equals(status?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static Result<string>? ValidateBidSubmission(SubmitBidRequest request)
    {
        if (request is null)
        {
            return Result<string>.Fail("invalid_request", "Bid request payload is required.");
        }

        if (request.BidAmount <= 0)
        {
            return Result<string>.Fail("invalid_bid_amount", "Bid amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return Result<string>.Fail("invalid_currency", "Currency is required for bid submission.");
        }

        if (request.ExpectedDeliveryDate is null)
        {
            return Result<string>.Fail("invalid_delivery_date", "Expected delivery date is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProposalSummary))
        {
            return Result<string>.Fail("invalid_proposal_summary", "Proposal summary is required.");
        }

        return null;
    }

    public static bool IsBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            _ = Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
