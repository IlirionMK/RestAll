using System.Text;
using System.Text.Json;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Kitchen;

public sealed class HttpKitchenGateway : IKitchenGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;

    public HttpKitchenGateway(HttpClient httpClient, RestAllApiOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<List<KitchenTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/kitchen/tickets", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new List<KitchenTicket>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<KitchenTicket>();
            }

            var tickets = new List<KitchenTicket>();
            foreach (var element in data.EnumerateArray())
            {
                var ticket = ParseTicket(element);
                if (ticket is not null)
                {
                    tickets.Add(ticket);
                }
            }

            return tickets;
        }
        catch
        {
            return new List<KitchenTicket>();
        }
    }

    public async Task<bool> UpdateTicketStatusAsync(int orderItemId, OrderItemStatus status, CancellationToken cancellationToken)
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

            var response = await _httpClient.PatchAsync($"{_options.BaseUrl}/kitchen/tickets/{orderItemId}/status", content, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private KitchenTicket? ParseTicket(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetIntProperty(element, "order_id", out var orderId) ||
            !JsonParserHelper.TryGetIntProperty(element, "menu_item_id", out var menuItemId) ||
            !JsonParserHelper.TryGetStringProperty(element, "name", out var name) ||
            !JsonParserHelper.TryGetDecimalProperty(element, "price", out var price) ||
            !JsonParserHelper.TryGetIntProperty(element, "quantity", out var quantity) ||
            !TryGetTicketStatus(element, out var status))
        {
            return null;
        }

        var comment = JsonParserHelper.TryGetStringProperty(element, "comment", out var comm) ? comm : null;
        var tableName = JsonParserHelper.TryGetNestedStringProperty(element, "table", "name", out var tableNameVal) ? tableNameVal : null;

        return new KitchenTicket(id, orderId, menuItemId, name ?? string.Empty, price, quantity, comment, status, tableName);
    }

    private static bool TryGetTicketStatus(JsonElement element, out OrderItemStatus status)
    {
        if (JsonParserHelper.TryGetIntProperty(element, "status", out var statusValue))
        {
            status = statusValue switch
            {
                0 => OrderItemStatus.Pending,
                1 => OrderItemStatus.Preparing,
                2 => OrderItemStatus.Ready,
                3 => OrderItemStatus.Served,
                _ => OrderItemStatus.Pending
            };
            return true;
        }

        if (JsonParserHelper.TryGetStringProperty(element, "status", out var statusText))
        {
            status = statusText?.ToLowerInvariant() switch
            {
                "pending" => OrderItemStatus.Pending,
                "preparing" => OrderItemStatus.Preparing,
                "ready" => OrderItemStatus.Ready,
                "served" => OrderItemStatus.Served,
                _ => OrderItemStatus.Pending
            };
            return true;
        }

        status = OrderItemStatus.Pending;
        return false;
    }
}
