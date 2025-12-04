using System.ComponentModel.DataAnnotations;

namespace Api.Models.Users;

public record CreateUserRequest(
    [Required] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string Role,
    string? ProcurementSubRole,
    bool ProcurementCanCreate = false,
    bool ProcurementCanDelete = false,
    bool ProcurementCanView = false,
    bool ProcurementCanEdit = false);
