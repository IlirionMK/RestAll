using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.Infrastructure.Realtime;

namespace RestAll.Desktop.Tests.Realtime;

public class RealtimeFunctionalTests
{
    [Fact]
    public async Task WebSocketRealtimeService_Receives_ItemReady_Event_From_Server()
    {
        const int wsPort = 6001;
        const int authPort = 6002;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        // start auth HTTP server using HttpListener
        var authListener = new HttpListener();
        authListener.Prefixes.Add($"http://localhost:{authPort}/");
        authListener.Start();
        var authLoop = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                var ctx = await authListener.GetContextAsync();
                if (ctx.Request.HttpMethod == "POST" && ctx.Request.Url!.AbsolutePath == "/broadcasting/auth")
                {
                    ctx.Response.ContentType = "application/json";
                    var resp = Encoding.UTF8.GetBytes("{\"auth\":\"test:signature\"}");
                    await ctx.Response.OutputStream.WriteAsync(resp, 0, resp.Length, cts.Token);
                    ctx.Response.OutputStream.Close();
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.Close();
                }
            }
        }, cts.Token);

        // start websocket server using HttpListener AcceptWebSocketAsync
        var wsListener = new HttpListener();
        wsListener.Prefixes.Add($"http://localhost:{wsPort}/");
        wsListener.Start();
        var wsLoop = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                var ctx = await wsListener.GetContextAsync();
                if (ctx.Request.IsWebSocketRequest && ctx.Request.Url!.AbsolutePath.StartsWith($"/app/"))
                {
                    var wsContext = await ctx.AcceptWebSocketAsync(subProtocol: null);
                    var ws = wsContext.WebSocket;

                    // Send connection established frame
                    var connFrame = new { @event = "pusher:connection_established", data = new { socket_id = "123.1" } };
                    var connJson = JsonSerializer.Serialize(connFrame);
                    var connBuffer = Encoding.UTF8.GetBytes(connJson);
                    await ws.SendAsync(new ArraySegment<byte>(connBuffer), WebSocketMessageType.Text, true, cts.Token);

                    var buffer = new byte[4096];
                    while (!cts.IsCancellationRequested && ws.State == WebSocketState.Open)
                    {
                        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }

                        var received = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        try
                        {
                            using var receivedDoc = JsonDocument.Parse(received);
                            var root = receivedDoc.RootElement;
                            // pusher subscribe frame
                            if (root.TryGetProperty("event", out var ev) || root.TryGetProperty("@event", out ev))
                            {
                                var evName = ev.GetString();
                                if (evName == "pusher:subscribe")
                                {
                                    // extract channel
                                    if (root.TryGetProperty("data", out var data) && data.TryGetProperty("channel", out var ch))
                                    {
                                        var channel = ch.GetString();

                                        // send an application-level item.ready event on that channel
                                        var appPayload = new { order_id = 42, table_number = (int?)null, item_name = "Soup", quantity = 1 };
                                        var appFrame = new { @event = "item.ready", channel = channel, data = appPayload };
                                        var appJson = JsonSerializer.Serialize(appFrame);
                                        var appBuf = Encoding.UTF8.GetBytes(appJson);
                                        await ws.SendAsync(new ArraySegment<byte>(appBuf), WebSocketMessageType.Text, true, cts.Token);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignore parsing errors
                        }
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.Close();
                }
            }
        }, cts.Token);

        // prepare dependencies for service
        var mockAuth = new Mock<IAuthenticateUserUseCase>();
        mockAuth.SetupGet(a => a.State).Returns(AuthFlowState.Authenticated);
        mockAuth.SetupGet(a => a.CurrentSession).Returns(UserSession.FromProfile("Test", "chef"));

        var mockProfile = new Mock<IManageProfileUseCase>();
        mockProfile.Setup(p => p.GetProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile(1, "Test User", "test@example.com", "chef", null));

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{authPort}") };
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var options = new RealtimeOptions { Host = "localhost", Port = wsPort, AuthEndpoint = $"http://localhost:{authPort}/broadcasting/auth" };

        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<WebSocketRealtimeService>>();

        var service = new WebSocketRealtimeService(logger.Object, mockAuth.Object, mockProfile.Object, httpClientFactory.Object, options);

        ItemReadyEventArgs? receivedArgs = null;
        var itemTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        service.ItemReady += (s, e) =>
        {
            receivedArgs = e;
            itemTcs.TrySetResult(true);
        };

        // connect and wait for event
        await service.ConnectAsync(cts.Token);

        var completed = await Task.WhenAny(itemTcs.Task, Task.Delay(TimeSpan.FromSeconds(5), cts.Token));
        Assert.True(completed == itemTcs.Task, "Timed out waiting for ItemReady event");
        Assert.NotNull(receivedArgs);
        Assert.Equal(42, receivedArgs!.OrderId);
        Assert.Equal("Soup", receivedArgs.ItemName);

        // cleanup
        await service.DisconnectAsync();
        cts.Cancel();
        authListener.Stop();
        wsListener.Stop();
    }
}


