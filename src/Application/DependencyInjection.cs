using Microsoft.Extensions.DependencyInjection;
using Application.Features.Suppliers;
using Application.Features.Rfx;
using Application.Interfaces.Services;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IRfxService, RfxService>();

        return services;
    }
}
