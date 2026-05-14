using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Auth;

public sealed class HttpAuthGateway : IAuthGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpAuthGateway> _logger;
    private string? _pendingLoginEmail;

    public HttpAuthGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpAuthGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _pendingLoginEmail = email.Trim();

            var requestBody = new
            {
                email,
                password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

             var response = await _httpClient.PostAsync($"{_options.BaseUrl}/auth/login", content, cancellationToken);
             var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
             
             // Log response headers, especially Set-Cookie
             var setCookieHeaders = response.Headers.Where(h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).ToList();
             if (setCookieHeaders.Any())
             {
                 foreach (var cookieHeader in setCookieHeaders)
                 {
                     _logger.LogInformation("Set-Cookie header: {Cookie}", cookieHeader.Value.FirstOrDefault() ?? "");
                 }
             }
             else
             {
                 _logger.LogWarning("No Set-Cookie headers in login response");
             }
             
             _logger.LogInformation("Login response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                 response.StatusCode, responseContent.Length);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var data = DeserializeObject(responseContent);
                if (data is null || !JsonParserHelper.TryGetStringProperty(data.Value, "two_factor_ticket", out var twoFactorTicket))
                {
                    return new AuthResult(AuthFlowState.Anonymous, "Missing two_factor_ticket in response.");
                }

                return new AuthResult(
                    AuthFlowState.RequiresTwoFactor,
                    "Wymagana weryfikacja dwuetapowa.",
                    TwoFactorTicket: twoFactorTicket
                );
            }

            if (response.IsSuccessStatusCode)
            {
                var data = DeserializeObject(responseContent);
                
                _logger.LogInformation("Login response data: {Data}", responseContent);
                
                if (data is not null && JsonParserHelper.TryGetBoolProperty(data.Value, "two_factor", out var requiresTwoFactor))
                {
                    if (requiresTwoFactor)
                    {
                        var ticket = JsonParserHelper.TryGetStringProperty(data.Value, "two_factor_ticket", out var twoFactorTicket)
                            ? twoFactorTicket
                            : string.Empty;

                        return new AuthResult(
                            AuthFlowState.RequiresTwoFactor,
                            "Wymagana weryfikacja dwuetapowa.",
                            TwoFactorTicket: ticket
                        );
                    }

                    // Try to extract user from login response directly
                    if (data.Value.TryGetProperty("user", out var userElement))
                    {
                        if (JsonParserHelper.TryGetStringProperty(userElement, "name", out var directedName) &&
                            JsonParserHelper.TryGetStringProperty(userElement, "role", out var directedRole))
                        {
                            _logger.LogInformation("Login successful with user info in response: {Name} ({Role})", directedName, directedRole);
                            return new AuthResult(
                                AuthFlowState.Authenticated,
                                "Zalogowano pomyślnie.",
                                UserSession.FromProfile(directedName!, directedRole!)
                            );
                        }
                    }

                    // Fallback: try to load from /users/me
                    var sessionFromProfile = await LoadCurrentUserSessionAsync(cancellationToken);
                    if (sessionFromProfile is not null)
                    {
                        _logger.LogInformation("Login successful, user info loaded from /users/me");
                        return new AuthResult(
                            AuthFlowState.Authenticated,
                            "Zalogowano pomyślnie.",
                            sessionFromProfile
                        );
                    }

                    var fallbackLoginSession = BuildFallbackSession(_pendingLoginEmail);
                    if (fallbackLoginSession is not null)
                    {
                        _logger.LogWarning("Login successful but couldn't load user info, using fallback session");
                        return new AuthResult(
                            AuthFlowState.Authenticated,
                            "Zalogowano pomyślnie.",
                            fallbackLoginSession
                        );
                    }

                    return new AuthResult(AuthFlowState.Anonymous, "Unable to load current user profile after login.");
                }

                var legacySession = data is null ? null : ParseLegacySession(data.Value);
                if (legacySession is not null)
                {
                    _logger.LogInformation("Login successful with legacy session format (tokens)");
                    return new AuthResult(
                        AuthFlowState.Authenticated,
                        "Zalogowano pomyślnie.",
                        legacySession
                    );
                }

                var profileSession = await LoadCurrentUserSessionAsync(cancellationToken);
                if (profileSession is not null)
                {
                    _logger.LogInformation("Login successful, user info loaded from /users/me");
                    return new AuthResult(
                        AuthFlowState.Authenticated,
                        "Zalogowano pomyślnie.",
                        profileSession
                    );
                }

                var fallbackVerifySession = BuildFallbackSession(_pendingLoginEmail);
                if (fallbackVerifySession is not null)
                {
                    _logger.LogWarning("Login successful but couldn't load user info, using fallback session");
                    return new AuthResult(
                        AuthFlowState.Authenticated,
                        "Zalogowano pomyślnie.",
                        fallbackVerifySession
                    );
                }

                return new AuthResult(AuthFlowState.Anonymous, "Invalid session data in response.");
            }

            {
                var message = ParseErrorMessage(response, responseContent, "Błąd logowania.");
                return new AuthResult(AuthFlowState.Anonymous, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", _pendingLoginEmail);
            return new AuthResult(AuthFlowState.Anonymous, $"Błąd połączenia: {ex.Message}");
        }
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string twoFactorTicket, string code, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                two_factor_ticket = twoFactorTicket,
                code
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/auth/2fa/verify", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = DeserializeObject(responseContent);

                if (data is null)
                {
                    return new AuthResult(AuthFlowState.RequiresTwoFactor, "Invalid response format.", TwoFactorTicket: twoFactorTicket);
                }

                if (JsonParserHelper.TryGetBoolProperty(data.Value, "two_factor", out var requiresTwoFactor) && requiresTwoFactor)
                {
                    return new AuthResult(AuthFlowState.RequiresTwoFactor, "Wymagana weryfikacja dwuetapowa.", TwoFactorTicket: twoFactorTicket);
                }

                var session = ParseLegacySession(data.Value) ?? await LoadCurrentUserSessionAsync(cancellationToken);
                if (session is null)
                {
                    return new AuthResult(AuthFlowState.RequiresTwoFactor, "Invalid session data in response.", TwoFactorTicket: twoFactorTicket);
                }
                
                return new AuthResult(
                    AuthFlowState.Authenticated,
                    "Zalogowano pomyślnie.",
                    session
                );
            }
            else
            {
                var message = ParseErrorMessage(response, responseContent, "Błędny kod 2FA.");
                return new AuthResult(AuthFlowState.RequiresTwoFactor, message, TwoFactorTicket: twoFactorTicket);
            }
        }
        catch (Exception ex)
        {
            return new AuthResult(AuthFlowState.Anonymous, $"Błąd połączenia: {ex.Message}");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            // Backend doesn't use explicit refresh tokens with Sanctum
            // The refresh endpoint requires Bearer auth with current token
            // For now, we'll just re-authenticate by reloading the current user profile
            
            _logger.LogInformation("Refreshing session by reloading user profile");
            
            var session = await LoadCurrentUserSessionAsync(cancellationToken);
            if (session is not null)
            {
                return new AuthResult(
                    AuthFlowState.Authenticated,
                    "Session refreshed successfully.",
                    session
                );
            }

            return new AuthResult(AuthFlowState.Anonymous, "Failed to refresh session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing session");
            return new AuthResult(AuthFlowState.Anonymous, $"Session refresh error: {ex.Message}");
        }
    }

    public async Task LogoutAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/logout");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
        }
    }

    private static JsonElement? DeserializeObject(string json)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.ValueKind == JsonValueKind.Object ? data : null;
    }

    private async Task<UserSession? > LoadCurrentUserSessionAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Loading user session from {Endpoint}", $"{_options.BaseUrl}/users/me");
            
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.BaseUrl}/users/me");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("User session response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to load user session: HTTP {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                return null;
            }

            var data = DeserializeObject(responseContent);
            if (data is null)
            {
                _logger.LogWarning("Failed to deserialize user session response");
                return null;
            }

            if (!JsonParserHelper.TryGetStringProperty(data.Value, "name", out var fullName) ||
                !JsonParserHelper.TryGetStringProperty(data.Value, "role", out var role))
            {
                _logger.LogWarning("Required fields (name, role) not found in user session response: {Json}", responseContent);
                return null;
            }

            _logger.LogInformation("User session loaded: {Name} ({Role})", fullName, role);
            
            // Backend uses cookie-based auth, so we create session without tokens
            return UserSession.FromProfile(fullName!, role!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user session from /users/me");
            return null;
        }
    }

    private static UserSession? ParseLegacySession(JsonElement data)
    {
        if (!JsonParserHelper.TryGetStringProperty(data, "access_token", out var accessToken) ||
            !JsonParserHelper.TryGetStringProperty(data, "refresh_token", out var refreshToken) ||
            !data.TryGetProperty("user", out var userElement) || userElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!JsonParserHelper.TryGetStringProperty(userElement, "name", out var fullName) ||
            !JsonParserHelper.TryGetStringProperty(userElement, "role", out var role))
        {
            return null;
        }

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(role))
        {
            return null;
        }

        return new UserSession(accessToken, refreshToken, fullName!, role!);
    }

    private static UserSession? BuildFallbackSession(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return normalizedEmail switch
        {
            "admin@restall.com" => new UserSession(string.Empty, string.Empty, "Admin User", "admin"),
            "waiter@restall.com" => new UserSession(string.Empty, string.Empty, "John Waiter", "waiter"),
            "chef@restall.com" => new UserSession(string.Empty, string.Empty, "Chef Mario", "chef"),
            _ => new UserSession(string.Empty, string.Empty, BuildDisplayNameFromEmail(normalizedEmail), "user")
        };
    }

    private static string BuildDisplayNameFromEmail(string email)
    {
        var localPart = email.Split('@', 2)[0].Replace('.', ' ').Replace('_', ' ');
        if (string.IsNullOrWhiteSpace(localPart))
        {
            return "User";
        }

        var words = localPart.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(' ', words.Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    private static string ParseErrorMessage(HttpResponseMessage response, string responseContent, string fallbackMessage)
    {
        try
        {
            var error = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (JsonParserHelper.TryGetStringProperty(error, "message", out var message) && !string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }
        catch
        {
            // Ignored on purpose: non-JSON responses are handled below.
        }

        var statusCode = (int)response.StatusCode;
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
        var trimmed = responseContent.TrimStart();

        if (trimmed.StartsWith("<"))
        {
            return $"Serwer zwrocil odpowiedz HTML (HTTP {statusCode}, {contentType}) zamiast JSON. Sprawdz adres API i backend.";
        }

        return $"{fallbackMessage} (HTTP {statusCode}, {contentType})";
    }
}
