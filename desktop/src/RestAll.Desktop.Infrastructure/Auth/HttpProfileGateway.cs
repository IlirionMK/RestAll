using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Auth;

public sealed class HttpProfileGateway : IProfileGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpProfileGateway> _logger;

    public HttpProfileGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpProfileGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<UserProfile?> GetProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching user profile from {Endpoint}", $"{_options.BaseUrl}/users/me");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.BaseUrl}/users/me");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Backend uses cookie-based auth, no bearer token needed
            // Cookie will be sent automatically by HttpClient

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("User profile response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch user profile: {Response}", responseContent);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseUserProfile(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            return null;
        }
    }

    public async Task<UserProfile?> UpdateProfileAsync(string accessToken, string name, string email, string role, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                name,
                email,
                role
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            using var request = new HttpRequestMessage(HttpMethod.Put, $"{_options.BaseUrl}/users/me");
            request.Content = content;
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Backend uses cookie-based auth, no bearer token needed
            // Cookie will be sent automatically by HttpClient

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update user profile: HTTP {StatusCode}", response.StatusCode);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseUserProfile(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return null;
        }
    }

    private static UserProfile? ParseUserProfile(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "name", out var name) ||
            !JsonParserHelper.TryGetStringProperty(element, "email", out var email) ||
            !JsonParserHelper.TryGetStringProperty(element, "role", out var role))
        {
            return null;
        }

        int? restaurantId = JsonParserHelper.TryGetIntProperty(element, "restaurant_id", out var restaurantIdValue)
            ? restaurantIdValue
            : null;

        return new UserProfile(
            id,
            name ?? string.Empty,
            email ?? string.Empty,
            role ?? string.Empty,
            restaurantId
        );
    }
}
