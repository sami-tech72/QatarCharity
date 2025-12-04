using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;

namespace Api.Models.Users;

public record UpdateUserRequest(
    [Required] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required] string Role,
    IEnumerable<string>? SubRoles);
