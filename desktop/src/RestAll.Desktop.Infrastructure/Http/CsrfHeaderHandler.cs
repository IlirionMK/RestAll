namespace RestAll.Desktop.Infrastructure.Http;

public sealed class CsrfHeaderHandler : DelegatingHandler
{
    private readonly ICsrfTokenService _csrfTokenService;

    public CsrfHeaderHandler(ICsrfTokenService csrfTokenService)
    {
        _csrfTokenService = csrfTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (RequiresCsrfToken(request.Method) && request.RequestUri is not null)
        {
            await _csrfTokenService.EnsureTokenAsync(request.RequestUri, cancellationToken);

            var token = _csrfTokenService.GetToken(request.RequestUri);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Remove("X-XSRF-TOKEN");
                request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", new string[] { token ?? string.Empty });
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static bool RequiresCsrfToken(HttpMethod method)
    {
        return method == HttpMethod.Post ||
               method == HttpMethod.Put ||
               method == HttpMethod.Patch ||
               method == HttpMethod.Delete;
    }
}



