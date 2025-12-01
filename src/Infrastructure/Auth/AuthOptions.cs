namespace Infrastructure.Auth;

public class AuthOptions
{
    public IList<UserCredentialOptions> Users { get; init; } = new List<UserCredentialOptions>();
}
