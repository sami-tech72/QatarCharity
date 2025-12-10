using Application.Features.ContractManagement.Commands;
using Application.Features.ContractManagement.Queries;
using Application.Features.Rfx.Commands;
using Application.Features.Rfx.Queries;
using Application.Features.Suppliers.Commands;
using Application.Features.Suppliers.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetSupplierBidsQuery>();
        services.AddScoped<GetRfxListQuery>();
        services.AddScoped<CreateRfxCommand>();
        services.AddScoped<GetRfxByIdQuery>();
        services.AddScoped<EvaluateBidCommand>();
        services.AddScoped<ApproveRfxCommand>();
        services.AddScoped<CloseRfxCommand>();
        services.AddScoped<GetPublishedRfxListQuery>();
        services.AddScoped<GetPublishedRfxByIdQuery>();
        services.AddScoped<SubmitBidCommand>();
        services.AddScoped<GetContractReadyBidsQuery>();
        services.AddScoped<GetContractsQuery>();
        services.AddScoped<CreateContractCommand>();

        services.AddScoped<GetSuppliersQuery>();
        services.AddScoped<CreateSupplierCommand>();
        services.AddScoped<UpdateSupplierCommand>();

        return services;
    }
}
