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
        await SeedProcurementRoleTemplatesAsync(dbContext);
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

    private static async Task SeedProcurementRoleTemplatesAsync(AppDbContext dbContext)
    {
        if (await dbContext.ProcurementPermissionDefinitions.AnyAsync())
        {
            return;
        }

        var permissions = new List<ProcurementPermissionDefinition>
        {
            new() { Name = "User Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Content Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Disputes Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Database Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Finance Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Reporting", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "API Control", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Repository Management", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
            new() { Name = "Payroll", DefaultRead = true, DefaultWrite = true, DefaultCreate = true },
        };

        var administrator = BuildTemplate(
            "Administrator",
            "Best for business owners and company administrators",
            totalUsers: 4,
            newUsers: 2,
            avatars: new[] { "300-6.jpg", "300-5.jpg", "300-11.jpg", "300-3.jpg" },
            permissions: permissions,
            actionOverride: _ => (true, true, true));

        var manager = BuildTemplate(
            "Manager",
            "Best for team leads to manage permissions",
            totalUsers: 5,
            newUsers: 2,
            avatars: new[] { "300-14.jpg", "300-2.jpg", "300-7.jpg", "300-8.jpg" },
            extraCount: 1,
            permissions: permissions,
            actionOverride: _ => (true, true, false));

        var users = BuildTemplate(
            "Users",
            "Best for standard users who need access to all standard features.",
            totalUsers: 8,
            newUsers: 4,
            avatars: new[] { "300-9.jpg", "300-10.jpg", "300-12.jpg", "300-13.jpg" },
            extraCount: 2,
            permissions: permissions,
            actionOverride: _ => (true, false, false));

        var support = BuildTemplate(
            "Support",
            "Best for employees who regularly refund payments",
            totalUsers: 3,
            newUsers: 2,
            avatars: new[] { "300-4.jpg", "300-1.jpg", "300-19.jpg" },
            permissions: permissions,
            actionOverride: _ => (true, false, false));

        var restrictedUser = BuildTemplate(
            "Restricted User",
            "Best for people who need restricted access to sensitive data",
            totalUsers: 4,
            newUsers: 1,
            avatars: new[] { "300-21.jpg", "300-23.jpg", "300-24.jpg", "300-25.jpg" },
            permissions: permissions,
            actionOverride: p => p.Name == "Reporting" ? (true, false, false) : (false, false, false));

        await dbContext.ProcurementPermissionDefinitions.AddRangeAsync(permissions);
        await dbContext.ProcurementRoleTemplates.AddRangeAsync(new[] { administrator, manager, users, support, restrictedUser });
        await dbContext.SaveChangesAsync();
    }

    private static ProcurementRoleTemplate BuildTemplate(
        string name,
        string description,
        int totalUsers,
        int newUsers,
        IEnumerable<string> avatars,
        IEnumerable<ProcurementPermissionDefinition> permissions,
        int? extraCount = null,
        Func<ProcurementPermissionDefinition, (bool read, bool write, bool create)>? actionOverride = null)
    {
        var template = new ProcurementRoleTemplate
        {
            Name = name,
            Description = description,
            TotalUsers = totalUsers,
            NewUsers = newUsers,
            ExtraCount = extraCount,
            Avatars = avatars.Select(a => new ProcurementRoleAvatar { FileName = a }).ToList(),
        };

        foreach (var permission in permissions)
        {
            var actions = actionOverride?.Invoke(permission) ?? (permission.DefaultRead, permission.DefaultWrite, permission.DefaultCreate);

            template.Permissions.Add(new ProcurementRolePermission
            {
                ProcurementPermissionDefinition = permission,
                CanRead = actions.read,
                CanWrite = actions.write,
                CanCreate = actions.create,
            });
        }

        return template;
    }
}
