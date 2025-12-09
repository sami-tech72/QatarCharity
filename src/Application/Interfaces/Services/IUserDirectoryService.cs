namespace Application.Interfaces.Services;

public interface IUserDirectoryService
{
    Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds);
}
