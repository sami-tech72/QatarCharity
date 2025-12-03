using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Api.Models;
using Api.Models.Suppliers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize(Roles = Roles.Admin)]
public class SuppliersController : ControllerBase
{
    private const string DefaultSupplierPassword = "P@ssw0rd!";

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public SuppliersController(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierResponse>>>> GetSuppliers([FromQuery] SupplierQueryParameters query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var search = query.Search?.Trim().ToLowerInvariant();

        var suppliersQuery = _dbContext.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            suppliersQuery = suppliersQuery.Where(supplier =>
                supplier.SupplierCode.ToLower().Contains(search) ||
                supplier.CompanyName.ToLower().Contains(search) ||
                supplier.RegistrationNumber.ToLower().Contains(search) ||
                supplier.PrimaryContactName.ToLower().Contains(search) ||
                supplier.PrimaryContactEmail.ToLower().Contains(search) ||
                (supplier.PortalUserEmail ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await suppliersQuery.CountAsync();

        var suppliers = await suppliersQuery
            .OrderByDescending(supplier => supplier.SubmissionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = suppliers.Select(MapToResponse).ToList();

        var pagedResult = new PagedResult<SupplierResponse>(response, totalCount, pageNumber, pageSize);

        return Ok(ApiResponse<PagedResult<SupplierResponse>>.Ok(pagedResult, "Suppliers retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> CreateSupplier(UpsertSupplierRequest request)
    {
        var statusValidation = ValidateStatus(request.Status);

        if (statusValidation is not null)
        {
            return statusValidation;
        }

        var (portalUser, errorResult) = await EnsurePortalUserAsync(
            request.HasPortalAccess,
            request.PortalUserEmail,
            request.PrimaryContactName,
            null);

        if (errorResult is not null)
        {
            return errorResult;
        }

        var categories = SerializeList(request.BusinessCategories);
        var categoryList = ParseList(categories);
        var documents = SerializeList(request.UploadedDocuments);

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = GenerateSupplierCode(),
            CompanyName = request.CompanyName.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            PrimaryContactName = request.PrimaryContactName.Trim(),
            PrimaryContactEmail = request.PrimaryContactEmail.Trim(),
            PrimaryContactPhone = request.PrimaryContactPhone.Trim(),
            BusinessCategories = categories,
            CompanyAddress = request.CompanyAddress.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            YearEstablished = request.YearEstablished,
            NumberOfEmployees = request.NumberOfEmployees,
            UploadedDocuments = documents,
            Category = categoryList.FirstOrDefault() ?? "General",
            ContactPerson = request.PrimaryContactName.Trim(),
            SubmissionDate = DateTime.UtcNow,
            Status = request.Status,
            HasPortalAccess = request.HasPortalAccess,
            PortalUserEmail = request.HasPortalAccess ? request.PortalUserEmail?.Trim() : null,
            PortalUserId = request.HasPortalAccess ? portalUser?.Id : null,
        };

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<SupplierResponse>.Ok(MapToResponse(supplier), "Supplier created successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> UpdateSupplier(Guid id, UpsertSupplierRequest request)
    {
        var supplier = await _dbContext.Suppliers.FindAsync(id);

        if (supplier is null)
        {
            return NotFound(ApiResponse<SupplierResponse>.Fail("Supplier not found.", "suppliers_not_found"));
        }

        var statusValidation = ValidateStatus(request.Status);

        if (statusValidation is not null)
        {
            return statusValidation;
        }

        var (portalUser, errorResult) = await EnsurePortalUserAsync(
            request.HasPortalAccess,
            request.PortalUserEmail,
            request.PrimaryContactName,
            id);

        if (errorResult is not null)
        {
            return errorResult;
        }

        var categories = SerializeList(request.BusinessCategories);
        var categoryList = ParseList(categories);
        var documents = SerializeList(request.UploadedDocuments);

        supplier.CompanyName = request.CompanyName.Trim();
        supplier.RegistrationNumber = request.RegistrationNumber.Trim();
        supplier.PrimaryContactName = request.PrimaryContactName.Trim();
        supplier.PrimaryContactEmail = request.PrimaryContactEmail.Trim();
        supplier.PrimaryContactPhone = request.PrimaryContactPhone.Trim();
        supplier.BusinessCategories = categories;
        supplier.CompanyAddress = request.CompanyAddress.Trim();
        supplier.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        supplier.YearEstablished = request.YearEstablished;
        supplier.NumberOfEmployees = request.NumberOfEmployees;
        supplier.UploadedDocuments = documents;
        supplier.Category = categoryList.FirstOrDefault() ?? "General";
        supplier.ContactPerson = request.PrimaryContactName.Trim();
        supplier.Status = request.Status;
        supplier.HasPortalAccess = request.HasPortalAccess;
        supplier.PortalUserEmail = request.HasPortalAccess ? request.PortalUserEmail?.Trim() : null;
        supplier.PortalUserId = request.HasPortalAccess ? portalUser?.Id : null;

        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<SupplierResponse>.Ok(MapToResponse(supplier), "Supplier updated successfully."));
    }

    private static SupplierResponse MapToResponse(Supplier supplier)
    {
        return new SupplierResponse
        {
            Id = supplier.Id,
            SupplierCode = supplier.SupplierCode,
            CompanyName = supplier.CompanyName,
            RegistrationNumber = supplier.RegistrationNumber,
            PrimaryContactName = supplier.PrimaryContactName,
            PrimaryContactEmail = supplier.PrimaryContactEmail,
            PrimaryContactPhone = supplier.PrimaryContactPhone,
            BusinessCategories = ParseList(supplier.BusinessCategories),
            CompanyAddress = supplier.CompanyAddress,
            Website = supplier.Website,
            YearEstablished = supplier.YearEstablished,
            NumberOfEmployees = supplier.NumberOfEmployees,
            UploadedDocuments = ParseList(supplier.UploadedDocuments),
            Category = supplier.Category,
            ContactPerson = supplier.ContactPerson,
            SubmissionDate = supplier.SubmissionDate.ToString("MM/dd/yyyy"),
            Status = supplier.Status,
            HasPortalAccess = supplier.HasPortalAccess,
            PortalUserEmail = supplier.PortalUserEmail,
        };
    }

    private static string SerializeList(IEnumerable<string> values)
    {
        var sanitized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());

        return string.Join(';', sanitized);
    }

    private static List<string> ParseList(string data)
    {
        return string.IsNullOrWhiteSpace(data)
            ? new List<string>()
            : data.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();
    }

    private static string GenerateSupplierCode()
    {
        var random = Random.Shared.Next(1000, 9999);
        return $"#SUB-{random}";
    }

    private ActionResult<ApiResponse<SupplierResponse>>? ValidateStatus(string status)
    {
        if (SupplierStatus.All.Contains(status))
        {
            return null;
        }

        return BadRequest(ApiResponse<SupplierResponse>.Fail(
            "Invalid status provided.",
            "suppliers_invalid_status"));
    }

    private async Task<(ApplicationUser? portalUser, ActionResult<ApiResponse<SupplierResponse>>? errorResult)> EnsurePortalUserAsync(
        bool hasPortalAccess,
        string? portalEmail,
        string contactName,
        Guid? currentSupplierId)
    {
        if (!hasPortalAccess)
        {
            return (null, null);
        }

        if (string.IsNullOrWhiteSpace(portalEmail))
        {
            return (null, BadRequest(ApiResponse<SupplierResponse>.Fail(
                "Portal user email is required when enabling portal access.",
                "suppliers_missing_portal_email")));
        }

        var trimmedEmail = portalEmail.Trim();
        var normalizedPortalEmail = trimmedEmail.ToLowerInvariant();

        var portalEmailInUse = await _dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier =>
                supplier.HasPortalAccess &&
                supplier.PortalUserEmail != null &&
                supplier.PortalUserEmail.ToLower() == normalizedPortalEmail &&
                (!currentSupplierId.HasValue || supplier.Id != currentSupplierId.Value));

        if (portalEmailInUse is not null)
        {
            return (null, Conflict(ApiResponse<SupplierResponse>.Fail(
                "This portal email is already linked to another supplier.",
                "suppliers_portal_email_in_use")));
        }

        var user = await _userManager.FindByEmailAsync(trimmedEmail);

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

            var createResult = await _userManager.CreateAsync(newUser, DefaultSupplierPassword);

            if (!createResult.Succeeded)
            {
                return (null, BadRequest(ApiResponse<SupplierResponse>.Fail(
                    "Unable to create a portal account for this supplier.",
                    "suppliers_portal_user_create_failed",
                    BuildErrorDetails(createResult))));
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, Roles.Supplier);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);

                return (null, BadRequest(ApiResponse<SupplierResponse>.Fail(
                    "Unable to assign the Supplier role to the portal user.",
                    "suppliers_portal_user_role_failed",
                    BuildErrorDetails(roleResult))));
            }

            user = newUser;
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains(Roles.Supplier))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, Roles.Supplier);

            if (!addRoleResult.Succeeded)
            {
                return (null, BadRequest(ApiResponse<SupplierResponse>.Fail(
                    "Portal user must have the Supplier role.",
                    "suppliers_portal_user_invalid_role",
                    BuildErrorDetails(addRoleResult))));
            }
        }

        var userInUse = await _dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier =>
                supplier.HasPortalAccess &&
                supplier.PortalUserId == user.Id &&
                (!currentSupplierId.HasValue || supplier.Id != currentSupplierId.Value));

        if (userInUse is not null)
        {
            return (null, Conflict(ApiResponse<SupplierResponse>.Fail(
                "This portal account is already linked to another supplier.",
                "suppliers_portal_user_in_use")));
        }

        return (user, null);
    }

    private static Dictionary<string, object?>? BuildErrorDetails(IdentityResult identityResult)
    {
        if (identityResult.Errors is null)
        {
            return null;
        }

        var builder = new StringBuilder();

        foreach (var error in identityResult.Errors)
        {
            builder.Append(error.Description).Append(' ');
        }

        return new Dictionary<string, object?>
        {
            ["errors"] = builder.ToString().Trim(),
        };
    }
}
