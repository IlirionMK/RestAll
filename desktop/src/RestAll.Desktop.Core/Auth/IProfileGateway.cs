namespace RestAll.Desktop.Core.Auth;

public interface IProfileGateway
{
    Task<UserProfile?> GetProfileAsync(string accessToken, CancellationToken cancellationToken);
    Task<UserProfile?> UpdateProfileAsync(string accessToken, string name, string email, string role, CancellationToken cancellationToken);
}
