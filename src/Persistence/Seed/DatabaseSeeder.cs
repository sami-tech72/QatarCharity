using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Domain.Entities.Procurement;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Identity;

namespace Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

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

        await SeedProcurementRolesAsync(dbContext);
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

   
    private static async Task SeedProcurementRolesAsync(AppDbContext dbContext)
    {
        var requiredDefinitions = new List<ProcurementPermissionDefinition>
        {
            new() { Name = "Roles & Permissions", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "RFx Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Bid Evaluation", DefaultRead = true, DefaultWrite = true, DefaultCreate = false },
            new() { Name = "Contract Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = false },
            new() { Name = "Supplier Performance", DefaultRead = true, DefaultWrite = false, DefaultCreate = false },
            new() { Name = "Reports & Analytics", DefaultRead = true, DefaultWrite = false, DefaultCreate = false },
        };

        var existingDefinitions = await dbContext.ProcurementPermissionDefinitions.ToListAsync();

        foreach (var definition in requiredDefinitions)
        {
            var existing = existingDefinitions.FirstOrDefault(d => d.Name == definition.Name);

            if (existing is null)
            {
                dbContext.ProcurementPermissionDefinitions.Add(definition);
            }
            else if (existing.DefaultRead != definition.DefaultRead ||
                     existing.DefaultWrite != definition.DefaultWrite ||
                     existing.DefaultCreate != definition.DefaultCreate)
            {
                existing.DefaultRead = definition.DefaultRead;
                existing.DefaultWrite = definition.DefaultWrite;
                existing.DefaultCreate = definition.DefaultCreate;
            }
        }

        await dbContext.SaveChangesAsync();

        // Role templates are intentionally not seeded.
    }

    
}
