using System.Net;

namespace RestAll.Desktop.Infrastructure.Http;

/// <summary>
/// Maintains a static shared CookieContainer used by all PersistentCookieHandler instances.
/// This ensures that session cookies are preserved and resent with each request.
/// </summary>
public sealed class PersistentCookieHandler : DelegatingHandler
{
    private static readonly CookieContainer SharedCookieContainer = new();

    public PersistentCookieHandler() : base(new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = SharedCookieContainer
    })
    {
    }

    /// <summary>
    /// Allows tests to access the shared cookie container for inspection.
    /// </summary>
    public static CookieContainer GetCookieContainer() => SharedCookieContainer;
}

