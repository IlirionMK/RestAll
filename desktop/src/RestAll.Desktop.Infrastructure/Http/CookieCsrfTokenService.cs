using System.Net;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace RestAll.Desktop.Infrastructure.Http;

public sealed class CookieCsrfTokenService : ICsrfTokenService
{
    private const string CsrfCookieName = "XSRF-TOKEN";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CookieContainer _cookieContainer;
    private readonly RestAll.Desktop.Infrastructure.Auth.RestAllApiOptions _options;
    private readonly ILogger<CookieCsrfTokenService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CookieCsrfTokenService(
        IHttpClientFactory httpClientFactory,
        CookieContainer cookieContainer,
        RestAll.Desktop.Infrastructure.Auth.RestAllApiOptions options,
        ILogger<CookieCsrfTokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cookieContainer = cookieContainer;
        _options = options;
        _logger = logger;
    }

    public async Task EnsureTokenAsync(Uri requestUri, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(GetToken(requestUri)))
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(GetToken(requestUri)))
            {
                return;
            }

            var client = _httpClientFactory.CreateClient("csrf-cookie");
            var csrfEndpoint = BuildCsrfCookieUrl();

            _logger.LogDebug("Fetching CSRF cookie from {Endpoint}", csrfEndpoint);
            using var response = await client.GetAsync(csrfEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public string? GetToken(Uri requestUri)
    {
        var cookies = _cookieContainer.GetCookies(requestUri);
        var csrfCookie = cookies
            .Cast<Cookie>()
            .FirstOrDefault(cookie => cookie.Name.Equals(CsrfCookieName, StringComparison.OrdinalIgnoreCase));

        if (csrfCookie is null || string.IsNullOrWhiteSpace(csrfCookie.Value))
        {
            return null;
        }

        return WebUtility.UrlDecode(csrfCookie.Value);
    }

    private string BuildCsrfCookieUrl()
    {
        return $"{BuildRootUrl(_options.BaseUrl)}/sanctum/csrf-cookie";
    }

    private static string BuildRootUrl(string apiBaseUrl)
    {
        var uri = new Uri(apiBaseUrl, UriKind.Absolute);
        return $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? string.Empty : $":{uri.Port}")}";
    }
}



