namespace RestAll.Desktop.Core.Auth;

public interface ISessionStorage
{
    Task<UserSession?> GetSessionAsync(CancellationToken cancellationToken = default);
    Task SaveSessionAsync(UserSession session, CancellationToken cancellationToken = default);
    Task ClearSessionAsync(CancellationToken cancellationToken = default);
}
