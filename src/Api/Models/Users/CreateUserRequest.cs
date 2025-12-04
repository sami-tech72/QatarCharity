using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;

namespace Api.Models.Users;

public record CreateUserRequest(
    [Required] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string Role,
    IEnumerable<string>? SubRoles);
