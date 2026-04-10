using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;
using Xunit;

namespace RestAll.Desktop.Tests.Auth;

public class HttpAuthGatewayTests
{
    [Fact]
    public async Task LoginAsync_WhenBackendReturnsFortifyLoginResponse_ShouldLoadUserProfile()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"two_factor\":false}", Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":1,\"name\":\"Admin User\",\"role\":\"admin\"}", Encoding.UTF8, "application/json")
            }
        );

        var client = new HttpClient(handler);
        var gateway = new HttpAuthGateway(client, new RestAllApiOptions { BaseUrl = "http://localhost/api" });

        var result = await gateway.LoginAsync("admin@restall.com", "password", CancellationToken.None);

        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Message.Should().Be("Zalogowano pomyślnie.");
        result.Session.Should().NotBeNull();
        result.Session!.FullName.Should().Be("Admin User");
        result.Session.Role.Should().Be("admin");
        result.Session.AccessToken.Should().BeEmpty();
        result.Session.RefreshToken.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginAsync_WhenProfileCannotBeLoaded_ShouldFallbackToKnownDesktopSession()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"two_factor\":false}", Encoding.UTF8, "application/json")
            }
        );

        var client = new HttpClient(handler);
        var gateway = new HttpAuthGateway(client, new RestAllApiOptions { BaseUrl = "http://localhost/api" });

        var result = await gateway.LoginAsync("admin@restall.com", "password", CancellationToken.None);

        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        result.Session!.FullName.Should().Be("Admin User");
        result.Session.Role.Should().Be("admin");
    }

    [Fact]
    public async Task LoginAsync_WhenBackendReturnsLegacyTokenPayload_ShouldParseSession()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"access\",\"refresh_token\":\"refresh\",\"user\":{\"name\":\"Admin User\",\"role\":\"admin\"}}", Encoding.UTF8, "application/json")
            }
        );

        var client = new HttpClient(handler);
        var gateway = new HttpAuthGateway(client, new RestAllApiOptions { BaseUrl = "http://localhost/api" });

        var result = await gateway.LoginAsync("admin@restall.com", "password", CancellationToken.None);

        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        result.Session!.AccessToken.Should().Be("access");
        result.Session.RefreshToken.Should().Be("refresh");
        result.Session.FullName.Should().Be("Admin User");
        result.Session.Role.Should().Be("admin");
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();

        public SequenceHttpMessageHandler(params HttpResponseMessage[] responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(_responses.Dequeue());
        }
    }
}

