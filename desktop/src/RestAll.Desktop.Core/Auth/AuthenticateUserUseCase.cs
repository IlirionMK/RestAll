using Microsoft.Extensions.Logging;

namespace RestAll.Desktop.Core.Auth;

public interface IAuthenticateUserUseCase
{
    AuthFlowState State { get; }
    UserSession? CurrentSession { get; }
    string? PendingTwoFactorTicket { get; }
    event EventHandler? SessionChanged;
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthResult> VerifyTwoFactorAsync(string code, CancellationToken cancellationToken);
    Task<AuthResult> RefreshTokenAsync(CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
    void ResetState();
    void InitializeAsync();
}

public sealed class AuthenticateUserUseCase : IAuthenticateUserUseCase
{
    private readonly IAuthGateway _gateway;
    private readonly ILogger<AuthenticateUserUseCase> _logger;
    private readonly ISessionStorage _sessionStorage;

    public AuthenticateUserUseCase(IAuthGateway gateway, ILogger<AuthenticateUserUseCase> logger, ISessionStorage sessionStorage)
    {
        _gateway = gateway;
        _logger = logger;
        _sessionStorage = sessionStorage;
    }

    public void InitializeAsync()
    {
        try
        {
            var session = _sessionStorage.GetSessionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (session is not null)
            {
                _logger.LogInformation("Loaded existing session from storage");
                State = AuthFlowState.Authenticated;
                CurrentSession = session;
                // Add delay to ensure session is fully set before firing event
                Task.Delay(200).ContinueWith(_ => SessionChanged?.Invoke(this, EventArgs.Empty));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading existing session from storage");
        }
    }

    public AuthFlowState State { get; private set; } = AuthFlowState.Anonymous;
    public UserSession? CurrentSession { get; private set; }
    public string? PendingTwoFactorTicket { get; private set; }
    public event EventHandler? SessionChanged;

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Login attempt with missing email or password");
            return new AuthResult(AuthFlowState.Anonymous, "Please provide email and password.");
        }

        _logger.LogInformation("Attempting login for email {Email}", email.Trim());
        var result = await _gateway.LoginAsync(email.Trim(), password, cancellationToken);
        
        if (result.State == AuthFlowState.Anonymous)
        {
            _logger.LogWarning("Login failed for email {Email}", email.Trim());
            ResetState();
        }
        else
        {
            _logger.LogInformation("Login successful for email {Email}", email.Trim());
            ApplyResult(result);
            // Save session to persistent storage
            await _sessionStorage.SaveSessionAsync(result.Session!, cancellationToken);
        }
        
        return result;
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string code, CancellationToken cancellationToken)
    {
        if (State != AuthFlowState.RequiresTwoFactor || string.IsNullOrWhiteSpace(PendingTwoFactorTicket))
        {
            _logger.LogWarning("2FA verification attempted without active 2FA step");
            return new AuthResult(AuthFlowState.Anonymous, "No active 2FA step.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("2FA verification attempted with empty code");
            return new AuthResult(AuthFlowState.RequiresTwoFactor, "Please enter 2FA code.", TwoFactorTicket: PendingTwoFactorTicket);
        }

        _logger.LogInformation("Attempting 2FA verification");
        var result = await _gateway.VerifyTwoFactorAsync(PendingTwoFactorTicket, code.Trim(), cancellationToken);
        
        if (result.State == AuthFlowState.Anonymous)
        {
            _logger.LogWarning("2FA verification failed");
            ResetState();
        }
        else
        {
            _logger.LogInformation("2FA verification successful");
            ApplyResult(result);
        }
        
        return result;
    }

    public async Task<AuthResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (CurrentSession is null)
        {
            _logger.LogWarning("Session refresh attempted without active session");
            return new AuthResult(AuthFlowState.Anonymous, "No active session.");
        }

        _logger.LogInformation("Attempting to refresh session");
        // Backend uses cookie-based auth, so we don't send a refresh_token
        // Just reload the current user profile to validate session
        var result = await _gateway.RefreshTokenAsync(string.Empty, cancellationToken);
        
        if (result.State == AuthFlowState.Anonymous)
        {
            _logger.LogWarning("Session refresh failed");
            ResetState();
        }
        else
        {
            _logger.LogInformation("Session refresh successful");
            ApplyResult(result);
            // Save refreshed session to persistent storage
            if (result.Session is not null)
            {
                await _sessionStorage.SaveSessionAsync(result.Session, cancellationToken);
            }
        }
        
        return result;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (CurrentSession is not null)
        {
            _logger.LogInformation("Logging out user");
            await _gateway.LogoutAsync(CurrentSession.AccessToken, cancellationToken);
        }

        _logger.LogInformation("User logged out successfully");
        ResetState();
        // Clear session from persistent storage
        await _sessionStorage.ClearSessionAsync(cancellationToken);
    }

    public void ResetState()
    {
        CurrentSession = null;
        PendingTwoFactorTicket = null;
        State = AuthFlowState.Anonymous;
        // Add delay to ensure session is fully set before firing event
                Task.Delay(200).ContinueWith(_ => SessionChanged?.Invoke(this, EventArgs.Empty));
    }

    private void ApplyResult(AuthResult result)
    {
        State = result.State;
        CurrentSession = result.Session;
        PendingTwoFactorTicket = result.TwoFactorTicket;
        // Add delay to ensure session is fully set before firing event
                Task.Delay(200).ContinueWith(_ => SessionChanged?.Invoke(this, EventArgs.Empty));
    }

    internal void ApplySessionForTesting(UserSession session)
    {
        State = AuthFlowState.Authenticated;
        CurrentSession = session;
        PendingTwoFactorTicket = null;
        // Add delay to ensure session is fully set before firing event
                Task.Delay(200).ContinueWith(_ => SessionChanged?.Invoke(this, EventArgs.Empty));
    }

    private void LoadExistingSessionAsync()
    {
        try
        {
            var session = _sessionStorage.GetSessionAsync().GetAwaiter().GetResult();
            if (session is not null)
            {
                _logger.LogInformation("Loaded existing session from storage");
                State = AuthFlowState.Authenticated;
                CurrentSession = session;
                // Add delay to ensure session is fully set before firing event
                Task.Delay(200).ContinueWith(_ => SessionChanged?.Invoke(this, EventArgs.Empty));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading existing session from storage");
        }
    }
}
