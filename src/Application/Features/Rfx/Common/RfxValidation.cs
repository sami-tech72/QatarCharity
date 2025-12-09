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
}
