using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

namespace Infrastructure.Services;

public class UserDirectoryService(UserManager<ApplicationUser> userManager) : IUserDirectoryService
{
    public async Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var users = await userManager.Users
            .Where(user => ids.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email, user.UserName })
            .ToListAsync();

        return users.ToDictionary(
            user => user.Id,
            user => user.DisplayName ?? user.Email ?? user.UserName ?? user.Id);
    }
}
