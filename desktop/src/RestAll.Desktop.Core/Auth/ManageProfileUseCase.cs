using Microsoft.Extensions.Logging;

namespace RestAll.Desktop.Core.Auth;

public sealed class ManageProfileUseCase : IManageProfileUseCase
{
    private readonly IProfileGateway _gateway;
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly ILogger<ManageProfileUseCase> _logger;

    public ManageProfileUseCase(IProfileGateway gateway, IAuthenticateUserUseCase authUseCase, ILogger<ManageProfileUseCase> logger)
    {
        _gateway = gateway;
        _authUseCase = authUseCase;
        _logger = logger;
    }

    public async Task<UserProfile?> GetProfileAsync(CancellationToken cancellationToken)
    {
        var sessionState = _authUseCase.State;
        var session = _authUseCase.CurrentSession;
        
        _logger.LogInformation("GetProfileAsync called - SessionState: {State}, Session: {Session}", 
            sessionState, session != null);
            
        if (sessionState != AuthFlowState.Authenticated || session is null)
        {
            _logger.LogWarning("Attempted to get profile without authenticated session. State: {State}", 
                sessionState);
            return null;
        }

        _logger.LogInformation("Fetching user profile using cookie-based session auth");
        // Backend uses cookie-based auth via Sanctum. No access token needed.
        return await _gateway.GetProfileAsync(string.Empty, cancellationToken);
    }

    public async Task<UserProfile?> UpdateProfileAsync(string name, string email, string role, CancellationToken cancellationToken)
    {
        var session = _authUseCase.CurrentSession;
        if (session is null)
        {
            _logger.LogWarning("Attempted to update profile without authenticated session");
            return null;
        }

        _logger.LogInformation("Updating user profile with name {Name}, email {Email}, role {Role}", name, email, role);
        // Backend uses cookie-based auth via Sanctum. No access token needed.
        return await _gateway.UpdateProfileAsync(string.Empty, name, email, role, cancellationToken);
    }
}
