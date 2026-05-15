using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Admin;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Admin;

public sealed class HttpAdminGateway : IAdminGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpAdminGateway> _logger;

    public HttpAdminGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpAdminGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<List<UserProfile>> GetStaffAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/users", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch staff: HTTP {StatusCode}", response.StatusCode);
                return new List<UserProfile>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(content);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<UserProfile>();
            }

            var result = new List<UserProfile>();
            foreach (var element in data.EnumerateArray())
            {
                var staff = ParseUserProfile(element);
                if (staff is not null)
                {
                    result.Add(staff);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching staff members");
            return new List<UserProfile>();
        }
    }

    public async Task<UserProfile?> CreateStaffAsync(string name, string email, string password, string role, CancellationToken cancellationToken)
    {
        try
        {
            var content = CreateJsonContent(new
            {
                name,
                email,
                password,
                role
            });

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/users", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create staff member: HTTP {StatusCode}", response.StatusCode);
                return null;
            }

            return ParseSingleUser(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member");
            return null;
        }
    }

    public async Task<UserProfile?> UpdateStaffRoleAsync(int userId, string role, CancellationToken cancellationToken)
    {
        try
        {
            var content = CreateJsonContent(new { role });
            var response = await _httpClient.PatchAsync($"{_options.BaseUrl}/users/{userId}/role", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update staff role for {UserId}: HTTP {StatusCode}", userId, response.StatusCode);
                return null;
            }

            return ParseSingleUser(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff role for {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> DeleteStaffAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_options.BaseUrl}/users/{userId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff member {UserId}", userId);
            return false;
        }
    }

    public async Task<AnalyticsSummary?> GetAnalyticsSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/analytics/summary", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch analytics summary: HTTP {StatusCode}", response.StatusCode);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(content);
            return data.ValueKind == JsonValueKind.Object ? ParseAnalyticsSummary(data) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics summary");
            return null;
        }
    }

    public async Task<AuditLogPage?> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = BuildAuditLogsEndpoint(query);
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch audit logs: HTTP {StatusCode}", response.StatusCode);
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(content);
            return data.ValueKind == JsonValueKind.Object ? ParseAuditLogPage(data) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            return null;
        }
    }

    private static StringContent CreateJsonContent(object body)
    {
        return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    }

    private static UserProfile? ParseSingleUser(string responseContent)
    {
        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return data.ValueKind == JsonValueKind.Object ? ParseUserProfile(data) : null;
        }
        catch
        {
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

        int? restaurantId = null;
        if (JsonParserHelper.TryGetIntProperty(element, "restaurant_id", out var restaurantIdValue))
        {
            restaurantId = restaurantIdValue;
        }

        return new UserProfile(id, name ?? string.Empty, email ?? string.Empty, role ?? string.Empty, restaurantId);
    }

    private static AnalyticsSummary ParseAnalyticsSummary(JsonElement element)
    {
        var revenue = element.TryGetProperty("revenue", out var revenueElement) && revenueElement.ValueKind == JsonValueKind.Object
            ? new AnalyticsRevenueStats(
                ReadDecimal(revenueElement, "today"),
                ReadDecimal(revenueElement, "this_week"),
                ReadDecimal(revenueElement, "this_month"))
            : new AnalyticsRevenueStats(0, 0, 0);

        var orders = element.TryGetProperty("orders", out var ordersElement) && ordersElement.ValueKind == JsonValueKind.Object
            ? new AnalyticsOrderStats(
                ReadInt(ordersElement, "today"),
                ReadInt(ordersElement, "this_week"),
                ReadInt(ordersElement, "this_month"),
                ReadDecimal(ordersElement, "average_value"))
            : new AnalyticsOrderStats(0, 0, 0, 0);

        var reservations = element.TryGetProperty("reservations", out var reservationsElement) && reservationsElement.ValueKind == JsonValueKind.Object
            ? new AnalyticsReservationStats(
                ReadInt(reservationsElement, "today"),
                ReadInt(reservationsElement, "this_week"))
            : new AnalyticsReservationStats(0, 0);

        var topItems = new List<AnalyticsTopItem>();
        if (element.TryGetProperty("top_items", out var topItemsElement) && topItemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var topItemElement in topItemsElement.EnumerateArray())
            {
                if (!JsonParserHelper.TryGetStringProperty(topItemElement, "name", out var name))
                {
                    continue;
                }

                topItems.Add(new AnalyticsTopItem(
                    name ?? string.Empty,
                    ReadInt(topItemElement, "quantity_sold"),
                    ReadDecimal(topItemElement, "revenue")
                ));
            }
        }

        return new AnalyticsSummary(revenue, orders, topItems, reservations);
    }

    private static AuditLogPage ParseAuditLogPage(JsonElement element)
    {
        var items = new List<AuditLogEntry>();
        if (element.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElement in dataElement.EnumerateArray())
            {
                var entry = ParseAuditLogEntry(itemElement);
                if (entry is not null)
                {
                    items.Add(entry);
                }
            }
        }

        return new AuditLogPage(
            items,
            ReadInt(element, "current_page", 1),
            ReadInt(element, "last_page", 1),
            ReadInt(element, "total", items.Count),
            ReadInt(element, "per_page", items.Count == 0 ? 50 : items.Count)
        );
    }

    private static AuditLogEntry? ParseAuditLogEntry(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "action", out var action))
        {
            return null;
        }

        int? userId = null;
        if (JsonParserHelper.TryGetIntProperty(element, "user_id", out var userIdValue))
        {
            userId = userIdValue;
        }

        string? userName = null;
        if (element.TryGetProperty("user", out var userElement) && userElement.ValueKind == JsonValueKind.Object)
        {
            userName = JsonParserHelper.TryGetStringProperty(userElement, "name", out var nestedName) ? nestedName : null;
        }

        string? payload = null;
        if (element.TryGetProperty("payload", out var payloadElement))
        {
            payload = payloadElement.ValueKind switch
            {
                JsonValueKind.String => payloadElement.GetString(),
                JsonValueKind.Object or JsonValueKind.Array => payloadElement.GetRawText(),
                JsonValueKind.Null => null,
                _ => payloadElement.GetRawText()
            };
        }

        int? modelId = null;
        if (JsonParserHelper.TryGetIntProperty(element, "model_id", out var modelIdValue))
        {
            modelId = modelIdValue;
        }

        string? modelType = JsonParserHelper.TryGetStringProperty(element, "model_type", out var mt) ? mt : null;
        string? ipAddress = JsonParserHelper.TryGetStringProperty(element, "ip_address", out var ip) ? ip : null;
        var createdAt = TryGetDateTime(element, "created_at", out var parsedCreatedAt) ? parsedCreatedAt : DateTime.MinValue;

        return new AuditLogEntry(id, userId, userName, action ?? string.Empty, modelType, modelId, payload, ipAddress, createdAt);
    }

    private string BuildAuditLogsEndpoint(AuditLogQuery query)
    {
        var builder = new StringBuilder($"{_options.BaseUrl}/logs");
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            queryParts.Add($"action={Uri.EscapeDataString(query.Action.Trim())}");
        }

        if (query.UserId is not null)
        {
            queryParts.Add($"user_id={query.UserId.Value}");
        }

        if (query.DateFrom is not null)
        {
            queryParts.Add($"date_from={Uri.EscapeDataString(query.DateFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        if (query.DateTo is not null)
        {
            queryParts.Add($"date_to={Uri.EscapeDataString(query.DateTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        queryParts.Add($"per_page={query.PerPage}");
        queryParts.Add($"page={query.Page}");

        if (queryParts.Count > 0)
        {
            builder.Append('?');
            builder.Append(string.Join('&', queryParts));
        }

        return builder.ToString();
    }

    private static int ReadInt(JsonElement element, string propertyName, int defaultValue = 0)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return defaultValue;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    private static decimal ReadDecimal(JsonElement element, string propertyName, decimal defaultValue = 0)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return defaultValue;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    private static bool TryGetDateTime(JsonElement element, string propertyName, out DateTime value)
    {
        value = default;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.String && DateTime.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }
}

