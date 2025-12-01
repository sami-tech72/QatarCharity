namespace Domain.Users;

public class UserAccount
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required string DisplayName { get; set; }

    public required string Role { get; set; }
}
