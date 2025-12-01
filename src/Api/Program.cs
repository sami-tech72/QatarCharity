using System.Security.Claims;
using Application.Authentication;
using Domain.Constants;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Seed;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are missing.");

builder.Services
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

builder.Services.AddAuthorization(options =>
{
    foreach (var role in Roles.All)
    {
        options.AddPolicy(role, policy => policy.RequireRole(role));
    }
});

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/login", async (
    LoginRequest request,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService tokenService) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

    if (!signInResult.Succeeded)
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var tokenResult = tokenService.CreateToken(user, roles);

    return Results.Ok(new LoginResponse(
        Email: user.Email ?? string.Empty,
        DisplayName: user.DisplayName ?? user.UserName ?? string.Empty,
        Role: roles.FirstOrDefault() ?? Roles.Supplier,
        Token: tokenResult.Token,
        ExpiresAt: tokenResult.ExpiresAt));
});

var authorizedGroup = app.MapGroup("/api").RequireAuthorization();

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

app.Run();

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Email,
    string DisplayName,
    string Role,
    string Token,
    DateTime ExpiresAt);
