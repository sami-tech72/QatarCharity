using System;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;

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

        var defaultProcurementSubRoles = new[]
        {
            "ProcurementManager",
            "ProcurementOfficer",
            "ProcurementViewer",
        };

        foreach (var subRole in defaultProcurementSubRoles)
        {
            if (await roleManager.RoleExistsAsync(subRole))
            {
                continue;
            }

            await roleManager.CreateAsync(new IdentityRole(subRole));
        }

        await EnsureUserExists(userManager, "admin@qcharity.test", "Admin User", Roles.Admin);
        await EnsureUserExists(userManager, "procurement@qcharity.test", "Procurement User", Roles.Procurement, defaultProcurementSubRoles);
        await EnsureUserExists(userManager, "supplier@qcharity.test", "Supplier User", Roles.Supplier);

        await SeedSuppliersAsync(dbContext, userManager);
    }

    private static async Task EnsureUserExists(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role,
        IEnumerable<string>? subRoles = null)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null)
        {
            var existingRoles = await userManager.GetRolesAsync(user);
            var targetRoles = new List<string> { role };

            if (role == Roles.Procurement && subRoles is not null)
            {
                targetRoles.AddRange(subRoles);
            }

            var missingRoles = targetRoles.Except(existingRoles);

            if (missingRoles.Any())
            {
                await userManager.AddToRolesAsync(user, missingRoles);
            }

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

        if (role == Roles.Procurement && subRoles is not null)
        {
            await userManager.AddToRolesAsync(user, subRoles);
        }
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
}
