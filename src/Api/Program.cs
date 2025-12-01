using Application.Auth;
using Domain.Users;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IAuthService, SqlAuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    await db.Database.EnsureCreatedAsync();

    if (!await db.UserAccounts.AnyAsync())
    {
        db.UserAccounts.AddRange(
            new UserAccount
            {
                Username = "admin",
                Password = "password123",
                DisplayName = "Admin User",
                Role = "Admin",
            },
            new UserAccount
            {
                Username = "procurement",
                Password = "procure!23",
                DisplayName = "Procurement Lead",
                Role = "Procurement",
            },
            new UserAccount
            {
                Username = "supplier",
                Password = "supply!23",
                DisplayName = "Supplier Partner",
                Role = "Supplier",
            });

        await db.SaveChangesAsync();
    }
}

app.UseHttpsRedirection();
app.UseCors("DevelopmentCors");

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Username and password are required." });
    }

    var result = await authService.AuthenticateAsync(request);

    return result is null
        ? Results.BadRequest(new { message = "Invalid username or password." })
        : Results.Ok(result);
})
.WithName("Login")
.WithOpenApi();

app.Run();
