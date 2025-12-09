using Application.DTOs.Authentication;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Persistence.Identity;

namespace Infrastructure.Services.Authentication;

public class SupplierPortalUserService(
    UserManager<ApplicationUser> userManager,
    ISupplierRepository supplierRepository)
    : ISupplierPortalUserService
{
    public async Task<PortalUserResult> EnsurePortalUserAsync(
        bool hasPortalAccess,
        string? portalEmail,
        string contactName,
        Guid? currentSupplierId)
    {
        if (!hasPortalAccess)
        {
            return PortalUserResult.Ok(string.Empty);
        }

        if (string.IsNullOrWhiteSpace(portalEmail))
        {
            return PortalUserResult.Fail("suppliers_missing_portal_email", "Portal user email is required when enabling portal access.");
        }

        var trimmedEmail = portalEmail.Trim();
        var normalizedPortalEmail = trimmedEmail.ToLowerInvariant();

        var portalEmailInUse = await supplierRepository.PortalEmailInUseAsync(normalizedPortalEmail, currentSupplierId);
        if (portalEmailInUse)
        {
            return PortalUserResult.Fail("suppliers_portal_email_in_use", "This portal email is already linked to another supplier.");
        }

        var user = await userManager.FindByEmailAsync(trimmedEmail);

        if (user is null)
        {
            var displayName = string.IsNullOrWhiteSpace(contactName)
                ? trimmedEmail
                : contactName.Trim();

            var newUser = new ApplicationUser
            {
                Email = trimmedEmail,
                UserName = trimmedEmail,
                EmailConfirmed = true,
                DisplayName = displayName,
            };

            var createResult = await userManager.CreateAsync(newUser, "P@ssw0rd!");

            if (!createResult.Succeeded)
            {
                return PortalUserResult.Fail(
                    "suppliers_portal_user_create_failed",
                    string.Join(' ', createResult.Errors.Select(error => error.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(newUser, Roles.Supplier);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(newUser);

                return PortalUserResult.Fail(
                    "suppliers_portal_user_role_failed",
                    string.Join(' ', roleResult.Errors.Select(error => error.Description)));
            }

            user = newUser;
        }

        var roles = await userManager.GetRolesAsync(user);

        if (!roles.Contains(Roles.Supplier))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, Roles.Supplier);

            if (!addRoleResult.Succeeded)
            {
                return PortalUserResult.Fail(
                    "suppliers_portal_user_invalid_role",
                    string.Join(' ', addRoleResult.Errors.Select(error => error.Description)));
            }
        }

        var userInUse = await supplierRepository.PortalUserInUseAsync(user.Id, currentSupplierId);
        if (userInUse)
        {
            return PortalUserResult.Fail("suppliers_portal_user_in_use", "This portal account is already linked to another supplier.");
        }

        return PortalUserResult.Ok(user.Id);
    }
}
