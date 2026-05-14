using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Orders;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Orders;

public class HttpOrderGatewayTests
{
    private readonly RestAllApiOptions _options = new RestAllApiOptions { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task GetOrdersAsync_WithSuccessResponse_ShouldReturnOrders()
    {
        // Arrange
        var responseJson = """
            [
                {
                    "id": 1,
                    "table_id": 1,
                    "user_id": 1,
                    "total": 25.98,
                    "status": "pending"
                },
                {
                    "id": 2,
                    "table_id": 2,
                    "user_id": 1,
                    "total": 15.99,
                    "status": "in_progress"
                }
            ]
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.GetOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrdersAsync_WithErrorResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.GetOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task PayOrderAsync_WithSuccessResponse_ShouldReturnTrue()
    {
        // Arrange
        var responseJson = """
            {
                "success": true
            }
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.PayOrderAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PayOrderAsync_WithErrorResponse_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.PayOrderAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }


    [Fact]
    public async Task CreateOrderAsync_WithErrorResponse_ShouldReturnNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpOrderGateway>>();
        var gateway = new HttpOrderGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.CreateOrderAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
