using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Infrastructure.Auth;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Auth;

public class HttpProfileGatewayTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly Mock<ILogger<HttpProfileGateway>> _loggerMock;
    private readonly HttpProfileGateway _gateway;

    public HttpProfileGatewayTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _options = new RestAllApiOptions { BaseUrl = "http://localhost/api" };
        _loggerMock = new Mock<ILogger<HttpProfileGateway>>();
        _gateway = new HttpProfileGateway(_httpClient, _options, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile()
    {
        // Arrange
        var accessToken = "test_token";
        var responseJson = JsonSerializer.Serialize(new
        {
            id = 1,
            name = "John Doe",
            email = "john@example.com",
            role = "waiter"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _gateway.GetProfileAsync(accessToken, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnNull_WhenRequestFails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act
        var result = await _gateway.GetProfileAsync("test_token", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnUpdatedProfile()
    {
        // Arrange
        var accessToken = "test_token";
        var responseJson = JsonSerializer.Serialize(new
        {
            id = 1,
            name = "John Smith",
            email = "john@example.com",
            role = "waiter"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _gateway.UpdateProfileAsync(accessToken, "John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Smith");
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnNull_WhenRequestFails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _gateway.UpdateProfileAsync("test_token", "John Smith", "john@example.com", "waiter", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
