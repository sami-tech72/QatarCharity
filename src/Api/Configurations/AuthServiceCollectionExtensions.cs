using System.Security.Claims;
using System.Text;
using Api.Authorization;
using Domain.Enums;
using Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api.Configurations;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are missing.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var role in Roles.All)
            {
                options.AddPolicy(role, policy => policy.RequireRole(role));
            }

            foreach (var (permission, action, policyName) in ProcurementPolicies.All)
            {
                options.AddPolicy(policyName, policy =>
                    policy.Requirements.Add(new ProcurementPermissionRequirement(permission, action)));
            }
        });

        services.AddSingleton<IAuthorizationHandler, ProcurementPermissionHandler>();

        return services;
    }
}
