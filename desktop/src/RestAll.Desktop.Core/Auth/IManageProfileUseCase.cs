namespace RestAll.Desktop.Core.Auth;

public interface IManageProfileUseCase
{
    Task<UserProfile?> GetProfileAsync(CancellationToken cancellationToken);
    Task<UserProfile?> UpdateProfileAsync(string name, string email, string role, CancellationToken cancellationToken);
}
