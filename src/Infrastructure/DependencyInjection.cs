using Application.Interfaces.Authentication;
using Application.Interfaces.Services;
using Infrastructure.Services.Authentication;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISupplierPortalUserService, SupplierPortalUserService>();
        services.AddScoped<IUserDirectoryService, UserDirectoryService>();

        return services;
    }
}
