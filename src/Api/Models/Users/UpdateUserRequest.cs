using System.ComponentModel.DataAnnotations;

namespace Api.Models.Users;

public record UpdateUserRequest(
    [Required] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required] string Role);
