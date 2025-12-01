using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserExists(userManager, "admin@qcharity.test", "Admin User", Roles.Admin);
        await EnsureUserExists(userManager, "procurement@qcharity.test", "Procurement User", Roles.Procurement);
        await EnsureUserExists(userManager, "supplier@qcharity.test", "Supplier User", Roles.Supplier);
    }

    private static async Task EnsureUserExists(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null)
        {
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
        };

        await userManager.CreateAsync(user, "P@ssw0rd!");
        await userManager.AddToRoleAsync(user, role);
    }
}
