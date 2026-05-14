using System.Net.WebSockets;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;

namespace RestAll.Desktop.Infrastructure.Realtime;

public sealed class WebSocketRealtimeService : IRealtimeService, IDisposable
{
    private const string UserChannelPrefix = "App.Models.User.";

    private readonly ILogger<WebSocketRealtimeService> _logger;
    private readonly IAuthenticateUserUseCase _authUseCase;
    private readonly IManageProfileUseCase _profileUseCase;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RealtimeOptions _options;
    private readonly SemaphoreSlim _stateGate = new(1, 1);

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runTask;
    private string? _socketId;

    public event EventHandler<OrderBillingRequestedEventArgs>? OrderBillingRequested;
    public event EventHandler<KitchenOrderItemsAddedEventArgs>? KitchenOrderItemsAdded;
    public event EventHandler<KitchenTicketStatusUpdatedEventArgs>? KitchenTicketStatusUpdated;
    public event EventHandler<ItemReadyEventArgs>? ItemReady;
    public event EventHandler<TestMessageEventArgs>? TestMessageReceived;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public WebSocketRealtimeService(
        ILogger<WebSocketRealtimeService> logger,
        IAuthenticateUserUseCase authUseCase,
        IManageProfileUseCase profileUseCase,
        IHttpClientFactory httpClientFactory,
        RealtimeOptions options)
    {
        _logger = logger;
        _authUseCase = authUseCase;
        _profileUseCase = profileUseCase;
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _stateGate.WaitAsync(cancellationToken);
        try
        {
            if (_runTask is { IsCompleted: false } || IsConnected)
            {
                _logger.LogInformation("Realtime connection is already active");
                return;
            }

            if (_authUseCase.State != AuthFlowState.Authenticated || _authUseCase.CurrentSession is null)
            {
                _logger.LogWarning("Realtime connection requested without authenticated session");
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runTask = RunAsync(_cancellationTokenSource.Token);
        }
        finally
        {
            _stateGate.Release();
        }

        await Task.CompletedTask;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _stateGate.WaitAsync(cancellationToken);
        try
        {
            _cancellationTokenSource?.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error while closing realtime websocket");
                }
            }
        }
        finally
        {
            _stateGate.Release();
        }

        if (_runTask is not null)
        {
            try
            {
                await _runTask;
            }
            catch (OperationCanceledException)
            {
                // expected during shutdown
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Realtime background task ended with error");
            }
        }

        CleanupSocket();
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsConnected);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var context = await BuildConnectionContextAsync(cancellationToken);
            if (context is null)
            {
                return;
            }

            _webSocket = new ClientWebSocket();
            _logger.LogInformation("Connecting realtime websocket to {Uri}", context.WebSocketUri);
            await _webSocket.ConnectAsync(context.WebSocketUri, cancellationToken);

            while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveTextMessageAsync(_webSocket, cancellationToken);
                if (message is null)
                {
                    break;
                }

                await HandleFrameAsync(message, context, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Realtime connection cancelled");
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error in realtime service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected realtime service error");
        }
        finally
        {
            CleanupSocket();
            _runTask = null;
            _socketId = null;
        }
    }

    private async Task<RealtimeConnectionContext?> BuildConnectionContextAsync(CancellationToken cancellationToken)
    {
        var profile = await _profileUseCase.GetProfileAsync(cancellationToken);
        if (profile is null)
        {
            _logger.LogWarning("Could not resolve user profile for realtime subscriptions");
            return null;
        }

        var channels = BuildChannels(profile);
        if (channels.Count == 0)
        {
            _logger.LogWarning("No realtime channels could be determined for role {Role}", profile.Role);
        }

        _logger.LogInformation("Realtime subscriptions prepared for user {UserId} in restaurant {RestaurantId} ({Role})",
            profile.Id, profile.RestaurantId, profile.Role);

        return new RealtimeConnectionContext(_options.BuildWebSocketUri(), channels);
    }

    private List<string> BuildChannels(UserProfile profile)
    {
        var channels = new List<string>();

        channels.Add($"private-{UserChannelPrefix}{profile.Id}");

        if (profile.RestaurantId is not null)
        {
            channels.Add($"private-restaurant.{profile.RestaurantId.Value}.staff");

            if (IsKitchenRole(profile.Role))
            {
                channels.Add($"private-restaurant.{profile.RestaurantId.Value}.kitchen");
            }
        }

        return channels.Distinct().ToList();
    }

    private static bool IsKitchenRole(string role)
    {
        return role.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("chef", StringComparison.OrdinalIgnoreCase);
    }

    private async Task HandleFrameAsync(string rawMessage, RealtimeConnectionContext context, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(rawMessage);
            var root = document.RootElement;

            if (!root.TryGetProperty("event", out var eventElement))
            {
                _logger.LogDebug("Ignoring websocket frame without event: {Frame}", Truncate(rawMessage));
                return;
            }

            var eventName = eventElement.GetString();
            var channelName = root.TryGetProperty("channel", out var channelElement) ? channelElement.GetString() : null;

            if (eventName is null)
            {
                return;
            }

            switch (eventName)
            {
                case "pusher:connection_established":
                    await HandleConnectionEstablishedAsync(root, context, cancellationToken);
                    return;

                case "pusher:ping":
                    await SendFrameAsync(new { @event = "pusher:pong" }, cancellationToken);
                    return;

                case "pusher:error":
                    _logger.LogWarning("Realtime websocket error frame: {Frame}", Truncate(rawMessage));
                    return;
            }

            if (eventName.StartsWith("pusher_internal:", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Internal websocket event {Event} on channel {Channel}", eventName, channelName);
                return;
            }

            if (channelName is null)
            {
                _logger.LogDebug("Skipping event without channel: {Event}", eventName);
                return;
            }

            if (!TryGetPayloadElement(root, out var payloadElement))
            {
                _logger.LogDebug("Skipping event with empty payload: {Event}", eventName);
                return;
            }

            await DispatchApplicationEventAsync(channelName, eventName, payloadElement, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse realtime frame: {Frame}", Truncate(rawMessage));
        }
    }

    private async Task HandleConnectionEstablishedAsync(JsonElement root, RealtimeConnectionContext context, CancellationToken cancellationToken)
    {
        if (!TryGetPayloadElement(root, out var payloadElement) ||
            !payloadElement.TryGetProperty("socket_id", out var socketIdElement) ||
            string.IsNullOrWhiteSpace(socketIdElement.GetString()))
        {
            _logger.LogWarning("Realtime connection established frame did not contain socket id");
            return;
        }

        _socketId = socketIdElement.GetString();
        _logger.LogInformation("Realtime socket established with socket id {SocketId}", _socketId);

        if (_socketId is null)
        {
            return;
        }

        foreach (var channel in context.Channels)
        {
            await SubscribeAsync(channel, _socketId, cancellationToken);
        }

#if DEBUG
        await SubscribeAsync("test-channel", _socketId, cancellationToken);
#endif
    }

    private async Task SubscribeAsync(string channelName, string socketId, CancellationToken cancellationToken)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            _logger.LogWarning("Cannot subscribe to {Channel} because websocket is not open", channelName);
            return;
        }

            if (channelName.StartsWith("private-", StringComparison.OrdinalIgnoreCase))
        {
            var auth = await AuthorizeChannelAsync(channelName, socketId, cancellationToken);
            if (string.IsNullOrWhiteSpace(auth))
            {
                _logger.LogWarning("Skipping subscription to {Channel} because auth failed", channelName);
                return;
            }

            await SendFrameAsync(new
            {
                    @event = "pusher:subscribe",
                data = new
                {
                    auth,
                    channel = channelName
                }
            }, cancellationToken);
        }
            else
            {
                await SendFrameAsync(new
                {
                    @event = "pusher:subscribe",
                    data = new
                    {
                        channel = channelName
                    }
                }, cancellationToken);
            }

        _logger.LogInformation("Subscribed to realtime channel {Channel}", channelName);
    }

    private async Task<string?> AuthorizeChannelAsync(string channelName, string socketId, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("broadcasting-auth");
            var response = await client.PostAsJsonAsync(_options.AuthEndpoint, new
            {
                socket_id = socketId,
                channel_name = channelName
            }, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Realtime channel auth failed for {Channel}. HTTP {StatusCode}: {Response}",
                    channelName, response.StatusCode, Truncate(content));
                return null;
            }

            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("auth", out var authElement))
            {
                _logger.LogWarning("Realtime auth response for {Channel} did not contain auth token", channelName);
                return null;
            }

            return authElement.GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing realtime channel {Channel}", channelName);
            return null;
        }
    }

    private async Task DispatchApplicationEventAsync(string channelName, string eventName, JsonElement payload, CancellationToken cancellationToken)
    {
        switch (eventName)
        {
            case "order.billing_requested":
                if (TryParseOrderBillingRequested(payload, out var billingRequested))
                {
                    _logger.LogInformation("Received billing requested for order {OrderId} on {Channel}", billingRequested.OrderId, channelName);
                    OrderBillingRequested?.Invoke(this, billingRequested);
                }
                return;

            case "kitchen.items_added":
                if (TryParseKitchenItemsAdded(payload, out var itemsAdded))
                {
                    _logger.LogInformation("Received items added for order {OrderId} on {Channel}", itemsAdded.OrderId, channelName);
                    KitchenOrderItemsAdded?.Invoke(this, itemsAdded);
                }
                return;

            case "kitchen.ticket_status_updated":
                if (TryParseKitchenTicketStatusUpdated(payload, out var ticketUpdated))
                {
                    _logger.LogInformation("Received kitchen ticket update for ticket {TicketId} on {Channel}", ticketUpdated.TicketId, channelName);
                    KitchenTicketStatusUpdated?.Invoke(this, ticketUpdated);
                }
                return;

            case "item.ready":
                if (TryParseItemReady(payload, out var itemReady))
                {
                    _logger.LogInformation("Received item ready notification for order {OrderId} on {Channel}", itemReady.OrderId, channelName);
                    ItemReady?.Invoke(this, itemReady);
                }
                return;

#if DEBUG
            case "test.event":
                if (TryParseTestMessage(payload, out var testMessage))
                {
                    _logger.LogInformation("Received test realtime message on {Channel}", channelName);
                    TestMessageReceived?.Invoke(this, testMessage);
                }
                return;
#endif
        }

        _logger.LogDebug("Unhandled realtime event {Event} on channel {Channel}", eventName, channelName);
    }

    private static bool TryParseOrderBillingRequested(JsonElement payload, out OrderBillingRequestedEventArgs result)
    {
        result = null!;

        if (!TryGetInt32(payload, "order_id", out var orderId)) return false;
        if (!TryGetNullableInt32(payload, "table_number", out var tableNumber)) return false;
        if (!TryGetDecimal(payload, "total_amount", out var totalAmount)) return false;
        if (!TryGetString(payload, "customer_name", out var customerName)) return false;

        result = new OrderBillingRequestedEventArgs(orderId, tableNumber, totalAmount, customerName);
        return true;
    }

    private static bool TryParseKitchenItemsAdded(JsonElement payload, out KitchenOrderItemsAddedEventArgs result)
    {
        result = null!;

        if (!TryGetInt32(payload, "order_id", out var orderId)) return false;
        if (!TryGetNullableInt32(payload, "table_number", out var tableNumber)) return false;
        if (!payload.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array) return false;

        var items = new List<KitchenOrderItemAddedDto>();
        foreach (var itemElement in itemsElement.EnumerateArray())
        {
            if (!TryGetInt32(itemElement, "id", out var id)) return false;
            if (!TryGetString(itemElement, "name", out var name)) return false;
            if (!TryGetInt32(itemElement, "quantity", out var quantity)) return false;
            var comment = TryGetNullableString(itemElement, "comment");
            if (!TryGetOrderItemStatus(itemElement, "status", out var status)) return false;

            items.Add(new KitchenOrderItemAddedDto(id, name, quantity, comment, status));
        }

        result = new KitchenOrderItemsAddedEventArgs(orderId, tableNumber, items);
        return true;
    }

    private static bool TryParseKitchenTicketStatusUpdated(JsonElement payload, out KitchenTicketStatusUpdatedEventArgs result)
    {
        result = null!;

        if (!TryGetInt32(payload, "id", out var ticketId)) return false;
        if (!TryGetInt32(payload, "order_id", out var orderId)) return false;
        if (!TryGetNullableInt32(payload, "table_number", out var tableNumber)) return false;
        if (!TryGetString(payload, "name", out var name)) return false;
        if (!TryGetInt32(payload, "quantity", out var quantity)) return false;
        var comment = TryGetNullableString(payload, "comment");
        if (!TryGetOrderItemStatus(payload, "status", out var status)) return false;

        result = new KitchenTicketStatusUpdatedEventArgs(ticketId, orderId, tableNumber, name, quantity, comment, status);
        return true;
    }

    private static bool TryParseItemReady(JsonElement payload, out ItemReadyEventArgs result)
    {
        result = null!;

        if (!TryGetInt32(payload, "order_id", out var orderId)) return false;
        if (!TryGetNullableInt32(payload, "table_number", out var tableNumber)) return false;
        if (!TryGetString(payload, "item_name", out var itemName)) return false;
        if (!TryGetInt32(payload, "quantity", out var quantity)) return false;

        result = new ItemReadyEventArgs(orderId, tableNumber, itemName, quantity);
        return true;
    }

#if DEBUG
    private static bool TryParseTestMessage(JsonElement payload, out TestMessageEventArgs result)
    {
        result = null!;

        if (!TryGetString(payload, "content", out var content)) return false;
        result = new TestMessageEventArgs(content);
        return true;
    }
#endif

    private async Task SendFrameAsync(object frame, CancellationToken cancellationToken)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            return;
        }

        var json = JsonSerializer.Serialize(frame);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task<string?> ReceiveTextMessageAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var ms = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            ms.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static bool TryGetPayloadElement(JsonElement envelope, out JsonElement payloadElement)
    {
        payloadElement = default;

        if (!envelope.TryGetProperty("data", out var dataElement))
        {
            return false;
        }

        if (dataElement.ValueKind == JsonValueKind.String)
        {
            var raw = dataElement.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(raw);
                payloadElement = doc.RootElement.Clone();
                return true;
            }
            catch
            {
                using var fallbackDoc = JsonDocument.Parse($"\"{raw.Replace("\"", "\\\"")}\"");
                payloadElement = fallbackDoc.RootElement.Clone();
                return true;
            }
        }

        payloadElement = dataElement;
        return true;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string? TryGetNullableString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static bool TryGetInt32(JsonElement element, string propertyName, out int value)
    {
        value = default;
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out value);
    }

    private static bool TryGetNullableInt32(JsonElement element, string propertyName, out int? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            value = null;
            return true;
        }

        if (property.TryGetInt32(out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static bool TryGetDecimal(JsonElement element, string propertyName, out decimal value)
    {
        value = default;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.TryGetDecimal(out value),
            JsonValueKind.String => decimal.TryParse(property.GetString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value),
            _ => false
        };
    }

    private static bool TryGetOrderItemStatus(JsonElement element, string propertyName, out OrderItemStatus status)
    {
        status = default;
        if (!TryGetString(element, propertyName, out var rawStatus))
        {
            return false;
        }

        return Enum.TryParse(rawStatus, true, out status);
    }

    private static string Truncate(string value, int maxLength = 500)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }

    private void CleanupSocket()
    {
        try
        {
            _webSocket?.Dispose();
        }
        catch
        {
            // ignore
        }

        try
        {
            _cancellationTokenSource?.Dispose();
        }
        catch
        {
            // ignore
        }

        _webSocket = null;
        _cancellationTokenSource = null;
    }

    public void Dispose()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
        }
        catch
        {
            // ignore
        }

        CleanupSocket();
        _stateGate.Dispose();
    }

    private sealed record RealtimeConnectionContext(Uri WebSocketUri, IReadOnlyList<string> Channels);
}
