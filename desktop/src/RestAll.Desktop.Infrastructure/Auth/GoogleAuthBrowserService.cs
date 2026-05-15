using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RestAll.Desktop.Infrastructure.Auth;

/// <summary>
/// Handles Google OAuth2 authentication for desktop applications using system browser and local callback server
/// </summary>
public class GoogleAuthBrowserService
{
    private readonly ILogger<GoogleAuthBrowserService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly int _callbackPort;

    public GoogleAuthBrowserService(ILogger<GoogleAuthBrowserService> logger, string clientId, string clientSecret, int callbackPort = 8765)
    {
        _logger = logger;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _callbackPort = callbackPort;
    }

    /// <summary>
    /// Initiates Google OAuth2 flow using system browser and waits for callback
    /// Returns true if authentication was successful
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        HttpListener? listener = null;

        try
        {
            // Setup local HTTP listener for OAuth callback
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{_callbackPort}/callback/");
            listener.Start();

            _logger.LogInformation("OAuth callback listener started on port {Port}", _callbackPort);

            // Build Google OAuth URL
            var redirectUri = $"http://localhost:{_callbackPort}/callback";
            var scope = Uri.EscapeDataString("openid email profile");
            var state = Guid.NewGuid().ToString("N");
            
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={_clientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&response_type=code" +
                $"&scope={scope}" +
                $"&state={state}" +
                $"&access_type=offline" +
                $"&prompt=consent";

            _logger.LogInformation("Opening browser for Google OAuth");

            // Open system browser
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            // Wait for callback with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(5));

            var context = await listener.GetContextAsync();
            
            try
            {
                var request = context.Request;
                var response = context.Response;

                _logger.LogInformation("OAuth callback received from {RemoteEndpoint}", request.RemoteEndPoint);

                // Extract authorization code from query string
                var queryString = request.Url?.Query ?? "";
                var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
                
                var code = queryParams["code"];
                var error = queryParams["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("OAuth error: {Error}", error);
                    await SendResponseAsync(response, false, $"Authentication failed: {error}");
                    tcs.SetResult(false);
                    return false;
                }

                if (string.IsNullOrEmpty(code))
                {
                    _logger.LogWarning("No authorization code received");
                    await SendResponseAsync(response, false, "No authorization code received");
                    tcs.SetResult(false);
                    return false;
                }

                _logger.LogInformation("Authorization code received, exchanging for tokens");

                // Exchange code for tokens
                var tokenSuccess = await ExchangeCodeForTokensAsync(code, redirectUri);
                
                if (tokenSuccess)
                {
                    await SendResponseAsync(response, true, "Authentication successful! You can close this window.");
                    tcs.SetResult(true);
                }
                else
                {
                    await SendResponseAsync(response, false, "Failed to exchange code for tokens");
                    tcs.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OAuth callback");
                tcs.SetResult(false);
            }
            finally
            {
                listener.Stop();
            }

            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("OAuth authentication timed out or was cancelled");
            listener?.Stop();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth authentication");
            listener?.Stop();
            return false;
        }
    }

    private async Task<bool> ExchangeCodeForTokensAsync(string code, string redirectUri)
    {
        try
        {
            using var httpClient = new HttpClient();
            
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Token exchange successful");
            
            // Note: In a real implementation, you would parse the token response
            // and store the access token securely. For now, we just log success.
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            return false;
        }
    }

    private static async Task SendResponseAsync(HttpListenerResponse response, bool success, string message)
    {
        var cssCommon = "font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0;";
        var html = success 
            ? $"<!DOCTYPE html><html><head><title>Authentication Successful</title><style>body {{{cssCommon} background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }}.container {{ text-align: center; background: white; padding: 40px; border-radius: 10px; box-shadow: 0 10px 40px rgba(0,0,0,0.2); }}h1 {{ color: #4CAF50; margin-bottom: 20px; }}p {{ color: #666; font-size: 16px; }}</style></head><body><div class=\"container\"><h1>&#10003; Success!</h1><p>{message}</p><p style=\"margin-top: 20px; font-size: 14px; color: #999;\">You can close this window and return to the application.</p></div></body></html>"
            : $"<!DOCTYPE html><html><head><title>Authentication Failed</title><style>body {{{cssCommon} background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }}.container {{ text-align: center; background: white; padding: 40px; border-radius: 10px; box-shadow: 0 10px 40px rgba(0,0,0,0.2); }}h1 {{ color: #f44336; margin-bottom: 20px; }}p {{ color: #666; font-size: 16px; }}</style></head><body><div class=\"container\"><h1>&#10007; Failed</h1><p>{message}</p><p style=\"margin-top: 20px; font-size: 14px; color: #999;\">Please try again or contact support.</p></div></body></html>";

        var buffer = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
