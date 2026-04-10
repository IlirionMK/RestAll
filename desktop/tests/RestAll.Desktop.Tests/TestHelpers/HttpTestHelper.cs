using System.Net;
using System.Net.Http;

namespace RestAll.Desktop.Tests.TestHelpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage? _response;
    private readonly Exception? _exception;
    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    public MockHttpMessageHandler(Exception exception)
    {
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        if (_exception != null)
        {
            throw _exception;
        }

        return Task.FromResult(_response ?? new HttpResponseMessage(HttpStatusCode.InternalServerError));
    }
}
