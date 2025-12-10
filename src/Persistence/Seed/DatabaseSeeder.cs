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

        await SeedSuppliersAsync(dbContext, userManager);
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

    private static async Task SeedSuppliersAsync(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        if (await dbContext.Suppliers.AnyAsync())
        {
            return;
        }

        var supplierUser = await userManager.FindByEmailAsync("supplier@qcharity.test");

        var seededSuppliers = new List<Supplier>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SupplierCode = "#SUB-8702",
                CompanyName = "Ibn Sina Medical Supplies",
                RegistrationNumber = "CR-102938",
                PrimaryContactName = "Dr. Amina Rahman",
                PrimaryContactEmail = "amina.rahman@ibnsina.qa",
                PrimaryContactPhone = "+974 4412 0001",
                BusinessCategories = "Medical;Pharmaceutical",
                CompanyAddress = "Building 12, Street 210, Industrial Area, Doha",
                Website = "https://ibnsinamed.qa",
                YearEstablished = 2008,
                NumberOfEmployees = 85,
                UploadedDocuments = "Trade License.pdf;Tax Certificate.pdf",
                Category = "Medical",
                ContactPerson = "Dr. Amina Rahman",
                SubmissionDate = DateTime.UtcNow.AddDays(-12),
                Status = SupplierStatus.Approved,
                HasPortalAccess = supplierUser is not null,
                PortalUserEmail = supplierUser?.Email,
                PortalUserId = supplierUser?.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                SupplierCode = "#SUB-5120",
                CompanyName = "Doha Logistics Partners",
                RegistrationNumber = "CR-548210",
                PrimaryContactName = "Yousef Al-Khaled",
                PrimaryContactEmail = "yousef.khaled@dohalogistics.com",
                PrimaryContactPhone = "+974 4488 3321",
                BusinessCategories = "Logistics;Warehousing",
                CompanyAddress = "Office 7, Ras Abu Aboud Street, Doha",
                Website = "https://dohalogistics.com",
                YearEstablished = 2014,
                NumberOfEmployees = 140,
                UploadedDocuments = "Safety Compliance.pdf",
                Category = "Logistics",
                ContactPerson = "Yousef Al-Khaled",
                SubmissionDate = DateTime.UtcNow.AddDays(-20),
                Status = SupplierStatus.Pending,
                HasPortalAccess = false,
                PortalUserEmail = null,
                PortalUserId = null,
            },
            new()
            {
                Id = Guid.NewGuid(),
                SupplierCode = "#SUB-4416",
                CompanyName = "Gulf Printworks",
                RegistrationNumber = "CR-776541",
                PrimaryContactName = "Mariam Al-Thani",
                PrimaryContactEmail = "mariam.t@gulfprintworks.qa",
                PrimaryContactPhone = "+974 4433 1188",
                BusinessCategories = "Print;Media",
                CompanyAddress = "Warehouse 3, Salwa Road, Doha",
                Website = "https://gulfprintworks.qa",
                YearEstablished = 2010,
                NumberOfEmployees = 65,
                UploadedDocuments = "Portfolio.pdf;Insurance.pdf",
                Category = "Print & Media",
                ContactPerson = "Mariam Al-Thani",
                SubmissionDate = DateTime.UtcNow.AddDays(-35),
                Status = SupplierStatus.Approved,
                HasPortalAccess = false,
                PortalUserEmail = null,
                PortalUserId = null,
            },
        };

        await dbContext.Suppliers.AddRangeAsync(seededSuppliers);
        await dbContext.SaveChangesAsync();
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

    private static List<ProcurementRolePermission> MapRolePermissions(
        IEnumerable<ProcurementPermissionDefinition> definitions,
        IReadOnlyDictionary<string, (bool read, bool write, bool create)> overrides)
    {
        return definitions
            .Select(definition =>
            {
                var hasOverride = overrides.TryGetValue(definition.Name, out var actions);
                var (read, write, create) = hasOverride
                    ? actions
                    : (definition.DefaultRead, definition.DefaultWrite, definition.DefaultCreate);

                return new ProcurementRolePermission
                {
                    ProcurementPermissionDefinitionId = definition.Id,
                    CanRead = read,
                    CanWrite = write,
                    CanCreate = create,
                };
            })
            .ToList();
    }
}
