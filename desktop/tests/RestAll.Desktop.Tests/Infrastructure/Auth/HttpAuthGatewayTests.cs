using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Auth;

public class HttpAuthGatewayTests
{
    private readonly RestAllApiOptions _options = new RestAllApiOptions { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task LoginAsync_WithSuccessResponse_ShouldReturnAuthenticated()
    {
        // Arrange
        var responseJson = """
            {
                "access_token": "test_access_token",
                "refresh_token": "test_refresh_token",
                "user": {
                    "name": "John Doe",
                    "role": "Admin"
                }
            }
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        result.Session!.AccessToken.Should().Be("test_access_token");
        result.Session.FullName.Should().Be("John Doe");
        result.Session.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task LoginAsync_WithTwoFactorRequired_ShouldReturnRequiresTwoFactor()
    {
        // Arrange
        var responseJson = """
            {
                "two_factor_ticket": "test_ticket_123"
            }
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.RequiresTwoFactor);
        result.TwoFactorTicket.Should().Be("test_ticket_123");
    }

    [Fact]
    public async Task LoginAsync_WithErrorResponse_ShouldReturnAnonymous()
    {
        // Arrange
        var responseJson = """
            {
                "message": "Invalid credentials"
            }
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        var result = await gateway.LoginAsync("test@example.com", "wrongpassword", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithNetworkError_ShouldReturnAnonymous()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");
        var handler = new MockHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Contain("Błąd połączenia");
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_WithSuccessResponse_ShouldReturnAuthenticated()
    {
        // Arrange
        var responseJson = """
            {
                "access_token": "test_access_token",
                "refresh_token": "test_refresh_token",
                "user": {
                    "name": "John Doe",
                    "role": "Admin"
                }
            }
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        var result = await gateway.VerifyTwoFactorAsync("test_ticket", "123456", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Authenticated);
        result.Session.Should().NotBeNull();
        result.Session!.AccessToken.Should().Be("test_access_token");
    }

    [Fact]
    public async Task LogoutAsync_ShouldNotThrowException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        await gateway.LogoutAsync("test_token", CancellationToken.None);

        // Assert
        // Should not throw exception
    }

    [Fact]
    public async Task LogoutAsync_WithError_ShouldNotThrowException()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");
        var handler = new MockHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpAuthGateway(httpClient, _options);

        // Act
        await gateway.LogoutAsync("test_token", CancellationToken.None);

        // Assert
        // Should not throw exception
    }
}
