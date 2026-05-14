using System.Net.Http.Headers;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.Infrastructure.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public AuthHeaderHandler(IAuthenticateUserUseCase authUseCase)
    {
        _authUseCase = authUseCase;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = _authUseCase.CurrentSession?.AccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If unauthorized, try to refresh token
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && 
            _authUseCase.CurrentSession?.RefreshToken != null)
        {
            // Only one refresh attempt at a time
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Check if another request already refreshed the token
                var newToken = _authUseCase.CurrentSession?.AccessToken;
                if (newToken != accessToken && !string.IsNullOrWhiteSpace(newToken))
                {
                    // Retry with new token
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response.Dispose();
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // Try to refresh the token
                    var refreshResult = await _authUseCase.RefreshTokenAsync(cancellationToken);
                    if (refreshResult.State == AuthFlowState.Authenticated)
                    {
                        // Retry with refreshed token
                        var refreshedToken = _authUseCase.CurrentSession?.AccessToken;
                        if (!string.IsNullOrWhiteSpace(refreshedToken))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken);
                            response.Dispose();
                            response = await base.SendAsync(request, cancellationToken);
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return response;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphore.Dispose();
        }
        base.Dispose(disposing);
    }
}
