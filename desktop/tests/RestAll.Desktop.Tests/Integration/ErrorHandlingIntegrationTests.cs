using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Menu;
using RestAll.Desktop.Infrastructure.Orders;
using RestAll.Desktop.Infrastructure.Tables;
using RestAll.Desktop.Infrastructure.Kitchen;
using RestAll.Desktop.Infrastructure.Reservations;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

public class ErrorHandlingIntegrationTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;

    public ErrorHandlingIntegrationTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _options = new RestAllApiOptions { BaseUrl = "http://localhost/api" };
    }

    [Fact]
    public async Task AuthGateway_WhenUnauthorized_ShouldReturnAnonymous()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpAuthGateway>>();
        var gateway = new HttpAuthGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new System.Net.Http.StringContent("{\"error\": \"Unauthorized\"}", System.Text.Encoding.UTF8, "application/json")
            });

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Contain("Błąd logowania");
    }

    [Fact]
    public async Task AuthGateway_WhenNetworkError_ShouldReturnAnonymous()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpAuthGateway>>();
        var gateway = new HttpAuthGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        result.Message.Should().Contain("Błąd połączenia");
    }

    [Fact]
    public async Task AuthGateway_WhenServerError_ShouldReturnAnonymous()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpAuthGateway>>();
        var gateway = new HttpAuthGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new System.Net.Http.StringContent("{\"error\": \"Internal server error\"}", System.Text.Encoding.UTF8, "application/json")
            });

        // Act
        var result = await gateway.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
    }

    [Fact]
    public async Task MenuGateway_WhenServerError_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpMenuGateway>>();
        var gateway = new HttpMenuGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await gateway.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task OrdersGateway_WhenUnauthorized_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(_httpClient, _options, logger.Object);

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
        var result = await gateway.GetOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TablesGateway_WhenNetworkError_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpTableGateway>>();
        var gateway = new HttpTableGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await gateway.GetTablesAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task KitchenGateway_WhenTimeout_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpKitchenGateway>>();
        var gateway = new HttpKitchenGateway(_httpClient, _options, logger.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result = await gateway.GetActiveTicketsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UseCase_WhenGatewayReturnsNull_ShouldHandleGracefully()
    {
        // Arrange
        var gatewayMock = new Mock<IAuthGateway>();
        var logger = new Mock<ILogger<AuthenticateUserUseCase>>();
        var sessionStorage = new Mock<ISessionStorage>();
        var useCase = new AuthenticateUserUseCase(gatewayMock.Object, logger.Object, sessionStorage.Object);

        gatewayMock.Setup(g => g.LoginAsync("test@example.com", "password", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthResult(AuthFlowState.Anonymous, "Error", null));

        // Act
        var result = await useCase.LoginAsync("test@example.com", "password", CancellationToken.None);

        // Assert
        result.State.Should().Be(AuthFlowState.Anonymous);
        useCase.State.Should().Be(AuthFlowState.Anonymous);
        useCase.CurrentSession.Should().BeNull();
    }

    [Fact]
    public async Task ProfileGateway_WhenUnauthorized_ShouldReturnNull()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpProfileGateway>>();
        var gateway = new HttpProfileGateway(_httpClient, _options, logger.Object);

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
        var result = await gateway.GetProfileAsync("test_token", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ReservationsGateway_WhenBadRequest_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = new Mock<ILogger<HttpReservationGateway>>();
        var gateway = new HttpReservationGateway(_httpClient, _options, logger.Object);

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
        var result = await gateway.GetReservationsAsync(DateTime.Now, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
