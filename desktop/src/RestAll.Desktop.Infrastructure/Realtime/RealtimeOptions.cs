namespace RestAll.Desktop.Infrastructure.Realtime;

public sealed class RealtimeOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 8080;
    public string AppKey { get; init; } = "restall_reverb_key";
    public string AuthEndpoint { get; init; } = "http://localhost:8000/broadcasting/auth";
    public string ClientName { get; init; } = "restall.desktop";
    public string ClientVersion { get; init; } = "1.0";

    public Uri BuildWebSocketUri()
    {
        var uri = new UriBuilder("ws", Host, Port, $"/app/{AppKey}")
        {
            Query = $"protocol=7&client={Uri.EscapeDataString(ClientName)}&version={Uri.EscapeDataString(ClientVersion)}&flash=false"
        };

        return uri.Uri;
    }
}

