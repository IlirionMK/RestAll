namespace RestAll.Desktop.Core.Auth;

public interface IAuthenticateUserUseCase
{
    AuthFlowState State { get; }
    UserSession? CurrentSession { get; }
    string? PendingTwoFactorTicket { get; }
    event EventHandler? SessionChanged;
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthResult> VerifyTwoFactorAsync(string code, CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
    void ResetState();
}

public sealed class AuthenticateUserUseCase : IAuthenticateUserUseCase
{
    private readonly IAuthGateway _gateway;

    public AuthenticateUserUseCase(IAuthGateway gateway)
    {
        _gateway = gateway;
    }

    public AuthFlowState State { get; private set; } = AuthFlowState.Anonymous;
    public UserSession? CurrentSession { get; private set; }
    public string? PendingTwoFactorTicket { get; private set; }
    public event EventHandler? SessionChanged;

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult(AuthFlowState.Anonymous, "Please provide email and password.");
        }

        var result = await _gateway.LoginAsync(email.Trim(), password, cancellationToken);
        
        if (result.State == AuthFlowState.Anonymous)
        {
            ResetState();
        }
        else
        {
            ApplyResult(result);
        }
        
        return result;
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string code, CancellationToken cancellationToken)
    {
        if (State != AuthFlowState.RequiresTwoFactor || string.IsNullOrWhiteSpace(PendingTwoFactorTicket))
        {
            return new AuthResult(AuthFlowState.Anonymous, "No active 2FA step.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return new AuthResult(AuthFlowState.RequiresTwoFactor, "Please enter 2FA code.", TwoFactorTicket: PendingTwoFactorTicket);
        }

        var result = await _gateway.VerifyTwoFactorAsync(PendingTwoFactorTicket, code.Trim(), cancellationToken);
        
        if (result.State == AuthFlowState.Anonymous)
        {
            ResetState();
        }
        else
        {
            ApplyResult(result);
        }
        
        return result;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (CurrentSession is not null)
        {
            await _gateway.LogoutAsync(CurrentSession.AccessToken, cancellationToken);
        }

        ResetState();
    }

    public void ResetState()
    {
        CurrentSession = null;
        PendingTwoFactorTicket = null;
        State = AuthFlowState.Anonymous;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyResult(AuthResult result)
    {
        State = result.State;
        CurrentSession = result.Session;
        PendingTwoFactorTicket = result.TwoFactorTicket;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
