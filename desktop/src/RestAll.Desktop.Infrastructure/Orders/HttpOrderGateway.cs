using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Json;

namespace RestAll.Desktop.Infrastructure.Orders;

public sealed class HttpOrderGateway : IOrderGateway
{
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly ILogger<HttpOrderGateway> _logger;

    public HttpOrderGateway(HttpClient httpClient, RestAllApiOptions options, ILogger<HttpOrderGateway> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching orders from {Endpoint}", $"{_options.BaseUrl}/orders");
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/orders", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Orders response: HTTP {StatusCode}, Content: {ContentLength} bytes", 
                response.StatusCode, responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch orders: {Response}", responseContent);
                return new List<Order>();
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Array)
            {
                return new List<Order>();
            }

            var orders = new List<Order>();
            foreach (var element in data.EnumerateArray())
            {
                var order = ParseOrder(element);
                if (order is not null)
                {
                    orders.Add(order);
                }
            }

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders from API");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/orders/{id}", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseOrder(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId} from API", id);
            return null;
        }
    }

    public async Task<Order?> CreateOrderAsync(int tableId, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                table_id = tableId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/orders", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseOrder(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for table {TableId}", tableId);
            return null;
        }
    }

    public async Task<Order?> AddOrderItemsAsync(int orderId, List<OrderItem> items, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                items = items.Select(i => new
                {
                    menu_item_id = i.MenuItemId,
                    quantity = i.Quantity,
                    comment = i.Comment
                })
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/orders/{orderId}/items", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ParseOrder(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding items to order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_options.BaseUrl}/orders/{orderId}/items/{orderItemId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item {OrderItemId} from order {OrderId}", orderItemId, orderId);
            return false;
        }
    }

    public async Task<bool> PayOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/orders/{orderId}/pay", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying order {OrderId}", orderId);
            return false;
        }
    }

    private Order? ParseOrder(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetIntProperty(element, "table_id", out var tableId) ||
            !JsonParserHelper.TryGetIntProperty(element, "user_id", out var userId) ||
            !TryGetTotalAmount(element, out var totalAmount))
        {
            return null;
        }

        var orderStatus = ParseOrderStatus(element);

        var items = new List<OrderItem>();
        if (element.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElement in itemsElement.EnumerateArray())
            {
                var item = ParseOrderItem(itemElement);
                if (item is not null)
                {
                    items.Add(item);
                }
            }
        }

        return new Order(id, tableId, userId, totalAmount, orderStatus, items);
    }

    private OrderItem? ParseOrderItem(JsonElement element)
    {
        if (!JsonParserHelper.TryGetIntProperty(element, "id", out var id) ||
            !JsonParserHelper.TryGetIntProperty(element, "order_id", out var orderId) ||
            !JsonParserHelper.TryGetIntProperty(element, "menu_item_id", out var menuItemId) ||
            !JsonParserHelper.TryGetStringProperty(element, "name", out var name) ||
            !JsonParserHelper.TryGetDecimalProperty(element, "price", out var price) ||
            !JsonParserHelper.TryGetIntProperty(element, "quantity", out var quantity) ||
            !TryGetOrderItemStatus(element, out var status))
        {
            return null;
        }

        var comment = JsonParserHelper.TryGetStringProperty(element, "comment", out var comm) ? comm : null;
        var itemStatus = status;

        return new OrderItem(id, orderId, menuItemId, name ?? string.Empty, price, quantity, comment, itemStatus);
    }

    private static bool TryGetTotalAmount(JsonElement element, out decimal totalAmount)
    {
        return JsonParserHelper.TryGetDecimalProperty(element, "total_amount", out totalAmount) ||
               JsonParserHelper.TryGetDecimalProperty(element, "total", out totalAmount);
    }

    private static OrderStatus ParseOrderStatus(JsonElement element)
    {
        if (JsonParserHelper.TryGetIntProperty(element, "status", out var statusValue))
        {
            return statusValue switch
            {
                0 => OrderStatus.Pending,
                1 => OrderStatus.InProgress,
                2 => OrderStatus.Paid,
                _ => OrderStatus.Pending
            };
        }

        if (JsonParserHelper.TryGetStringProperty(element, "status", out var statusText))
        {
            return statusText?.ToLowerInvariant() switch
            {
                "pending" => OrderStatus.Pending,
                "in_progress" => OrderStatus.InProgress,
                "paid" => OrderStatus.Paid,
                _ => OrderStatus.Pending
            };
        }

        return OrderStatus.Pending;
    }

    private static bool TryGetOrderItemStatus(JsonElement element, out OrderItemStatus status)
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
