using Application.Auth;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;

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

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorization();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IAuthService, IdentityAuthService>();

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

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await EnsureRoleAsync(roleManager, "Admin");
    await EnsureRoleAsync(roleManager, "Procurement");
    await EnsureRoleAsync(roleManager, "Supplier");

    await EnsureUserAsync(userManager, "admin", "Admin User", "password123", "Admin");
    await EnsureUserAsync(userManager, "procurement", "Procurement Lead", "procure!23", "Procurement");
    await EnsureUserAsync(userManager, "supplier", "Supplier Partner", "supply!23", "Supplier");
}

app.UseHttpsRedirection();
app.UseCors("DevelopmentCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();

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

static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string username, string displayName, string password, string role)
{
    var existing = await userManager.FindByNameAsync(username);
    if (existing is not null)
    {
        return;
    }

    var user = new ApplicationUser
    {
        UserName = username,
        Email = $"{username}@example.com",
        DisplayName = displayName,
        EmailConfirmed = true,
    };

    var result = await userManager.CreateAsync(user, password);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, role);
    }
}
