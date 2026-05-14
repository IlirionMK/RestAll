namespace RestAll.Desktop.Infrastructure.Http;

public interface ICsrfTokenService
{
    Task EnsureTokenAsync(Uri requestUri, CancellationToken cancellationToken);
    string? GetToken(Uri requestUri);
}

