using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

namespace Infrastructure.Services;

public class UserDirectoryService : IUserDirectoryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDirectoryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new();

        var users = await _userManager.Users
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.ToDictionary(
            u => u.Id,
            u => u.DisplayName ?? u.Email ?? u.UserName ?? u.Id
        );
    }
}
