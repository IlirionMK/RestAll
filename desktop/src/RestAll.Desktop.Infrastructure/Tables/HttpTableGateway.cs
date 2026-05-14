using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Tables;

public sealed class HttpTableGateway : ITableGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpTableGateway> _logger;

    public HttpTableGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpTableGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<List<Table>> GetTablesAsync(int restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl}/tables?restaurant_id={restaurantId}";
            _logger.LogInformation("Fetching tables from {Endpoint}", endpoint);
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Tables response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch tables: {Response}", responseContent);
                return new List<Table>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<Table>();
            }

            var tables = new List<Table>();
            foreach (var element in data.EnumerateArray())
            {
                var table = ParseTable(element);
                if (table is not null)
                {
                    tables.Add(table);
                }
            }

            return tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tables from API");
            return new List<Table>();
        }
    }

    public async Task<bool> UpdateTableStatusAsync(int id, TableStatus status, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                status = (int)status
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PatchAsync($"{_options.BaseUrl}/tables/{id}/status", content, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table {TableId} status to {Status}", id, status);
            return false;
        }
    }

    private Table? ParseTable(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetStringProperty(element, "number", out var number) ||
            !JsonParserHelper.TryGetIntProperty(element, "capacity", out var capacity))
        {
            return null;
        }

        var tableStatus = ParseTableStatus(element);

        return new Table(id, number ?? string.Empty, capacity, tableStatus);
    }

    private static TableStatus ParseTableStatus(JsonElement element)
    {
        if (JsonParserHelper.TryGetIntProperty(element, "status", out var statusValue))
        {
            return statusValue switch
            {
                0 => TableStatus.Available,
                1 => TableStatus.Occupied,
                2 => TableStatus.Reserved,
                3 => TableStatus.Cleaning,
                _ => TableStatus.Available
            };
        }

        if (JsonParserHelper.TryGetStringProperty(element, "status", out var statusText))
        {
            return statusText?.ToLowerInvariant() switch
            {
                "available" => TableStatus.Available,
                "occupied" => TableStatus.Occupied,
                "reserved" => TableStatus.Reserved,
                "cleaning" => TableStatus.Cleaning,
                _ => TableStatus.Available
            };
        }

        return TableStatus.Available;
    }
}
