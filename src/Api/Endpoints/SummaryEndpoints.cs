using Domain.Constants;

namespace Api.Endpoints;

public static class SummaryEndpoints
{
    public static IEndpointRouteBuilder MapSummaryEndpoints(this IEndpointRouteBuilder group)
    {
        var authorizedGroup = group.MapGroup(string.Empty).RequireAuthorization();

        authorizedGroup.MapGet("/admin/summary", () =>
            Results.Ok(new
            {
                Message = "Welcome, Admin",
                Timestamp = DateTimeOffset.UtcNow
            }))
            .RequireAuthorization(Roles.Admin)
            .WithName("AdminSummary");

        authorizedGroup.MapGet("/procurement/summary", () =>
            Results.Ok(new
            {
                Message = "Welcome, Procurement",
                Timestamp = DateTimeOffset.UtcNow
            }))
            .RequireAuthorization(Roles.Procurement)
            .WithName("ProcurementSummary");

        authorizedGroup.MapGet("/supplier/summary", () =>
            Results.Ok(new
            {
                Message = "Welcome, Supplier",
                Timestamp = DateTimeOffset.UtcNow
            }))
            .RequireAuthorization(Roles.Supplier)
            .WithName("SupplierSummary");

        return group;
    }
}
