namespace RestAll.Desktop.Core.Auth;

public interface IAuthGateway
{
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthResult> VerifyTwoFactorAsync(string twoFactorTicket, string code, CancellationToken cancellationToken);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string accessToken, CancellationToken cancellationToken);
    Task<bool> SendPasswordResetLinkAsync(string email, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(string email, string token, string password, string passwordConfirmation, CancellationToken cancellationToken);
}
