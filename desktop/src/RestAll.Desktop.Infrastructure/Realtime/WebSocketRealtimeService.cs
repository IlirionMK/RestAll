using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Infrastructure.Realtime;

public class WebSocketRealtimeService : IRealtimeService, IDisposable
{
    private readonly ILogger<WebSocketRealtimeService> _logger;
    private ClientWebSocket? _webSocket;
    private readonly string _baseUrl;
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<OrderUpdatedEventArgs>? OrderUpdated;
    public event EventHandler<OrderItemStatusUpdatedEventArgs>? OrderItemStatusUpdated;
    public event EventHandler<KitchenTicketUpdatedEventArgs>? KitchenTicketUpdated;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public WebSocketRealtimeService(ILogger<WebSocketRealtimeService> logger)
    {
        _logger = logger;
        _baseUrl = "ws://localhost:8000/ws";
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                _logger.LogWarning("WebSocket is already connected");
                return;
            }

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await _webSocket.ConnectAsync(new Uri(_baseUrl), _cancellationTokenSource.Token);
            
            if (_webSocket.State == WebSocketState.Open)
            {
                _logger.LogInformation("WebSocket connected to {BaseUrl}", _baseUrl);
                _ = StartListening(_cancellationTokenSource.Token);
            }
            else
            {
                _logger.LogError("Failed to connect to WebSocket");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to WebSocket");
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                _cancellationTokenSource?.Cancel();
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken);
                _webSocket = null;
                _logger.LogInformation("WebSocket disconnected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting WebSocket");
        }
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsConnected);
    }

    private async Task StartListening(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        
        while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket error while listening");
                break;
            }
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(message);
            
            if (data.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                
                switch (type)
                {
                    case "order.updated":
                        HandleOrderUpdated(data);
                        break;
                    case "order.item.status_updated":
                        HandleOrderItemStatusUpdated(data);
                        break;
                    case "kitchen.ticket.updated":
                        HandleKitchenTicketUpdated(data);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
        }
    }

    private void HandleOrderUpdated(JsonElement data)
    {
        if (data.TryGetProperty("order_id", out var orderIdElement) &&
            data.TryGetProperty("order", out var orderElement))
        {
            var orderId = orderIdElement.GetInt32();
            var order = JsonSerializer.Deserialize<Order>(orderElement.GetRawText());
            
            if (order != null)
            {
                _logger.LogInformation("Order {OrderId} updated via WebSocket", orderId);
                OrderUpdated?.Invoke(this, new OrderUpdatedEventArgs(orderId, order));
            }
        }
    }

    private void HandleOrderItemStatusUpdated(JsonElement data)
    {
        if (data.TryGetProperty("order_id", out var orderIdElement) &&
            data.TryGetProperty("order_item_id", out var orderItemIdElement) &&
            data.TryGetProperty("status", out var statusElement))
        {
            var orderId = orderIdElement.GetInt32();
            var orderItemId = orderItemIdElement.GetInt32();
            var status = statusElement.GetInt32();
            
            if (Enum.TryParse<OrderItemStatus>(status.ToString(), out var orderStatus))
            {
                _logger.LogInformation("Order item {OrderItemId} in order {OrderId} status updated to {Status} via WebSocket", 
                    orderItemId, orderId, orderStatus);
                OrderItemStatusUpdated?.Invoke(this, new OrderItemStatusUpdatedEventArgs(orderId, orderItemId, orderStatus));
            }
        }
    }

    private void HandleKitchenTicketUpdated(JsonElement data)
    {
        if (data.TryGetProperty("order_item_id", out var orderItemIdElement) &&
            data.TryGetProperty("status", out var statusElement))
        {
            var orderItemId = orderItemIdElement.GetInt32();
            var status = statusElement.GetInt32();
            
            if (Enum.TryParse<OrderItemStatus>(status.ToString(), out var orderStatus))
            {
                _logger.LogInformation("Kitchen ticket {OrderItemId} status updated to {Status} via WebSocket", 
                    orderItemId, orderStatus);
                KitchenTicketUpdated?.Invoke(this, new KitchenTicketUpdatedEventArgs(orderItemId, orderStatus));
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
