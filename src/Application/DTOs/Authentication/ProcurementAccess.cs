namespace Application.DTOs.Authentication;

public record ProcurementAccess(
    string[] SubRoles,
    string[] Permissions);
