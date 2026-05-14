namespace RestAll.Desktop.Core.Auth;

public interface IAuthGateway
{
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthResult> VerifyTwoFactorAsync(string twoFactorTicket, string code, CancellationToken cancellationToken);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string accessToken, CancellationToken cancellationToken);
}
