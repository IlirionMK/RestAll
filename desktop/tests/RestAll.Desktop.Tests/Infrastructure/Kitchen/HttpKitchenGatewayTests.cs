using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Kitchen;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Kitchen;

public class HttpKitchenGatewayTests
{
    private readonly RestAllApiOptions _options = new RestAllApiOptions { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task GetActiveTicketsAsync_WithSuccessResponse_ShouldReturnTickets()
    {
        // Arrange
        var responseJson = """
            [
                {
                    "id": 1,
                    "order_id": 1,
                    "menu_item_id": 1,
                    "name": "Burger",
                    "price": 15.99,
                    "quantity": 2,
                    "comment": "No onions",
                    "status": 0,
                    "table": {
                        "name": "Table 1"
                    }
                },
                {
                    "id": 2,
                    "order_id": 1,
                    "menu_item_id": 2,
                    "name": "Fries",
                    "price": 5.99,
                    "quantity": 1,
                    "comment": null,
                    "status": 1,
                    "table": {
                        "name": "Table 1"
                    }
                }
            ]
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpKitchenGateway>>();
        var gateway = new HttpKitchenGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.GetActiveTicketsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].OrderId.Should().Be(1);
        result[0].MenuItemId.Should().Be(1);
        result[0].MenuItemName.Should().Be("Burger");
        result[0].Price.Should().Be(15.99m);
        result[0].Quantity.Should().Be(2);
        result[0].Comment.Should().Be("No onions");
        result[0].Status.Should().Be(OrderItemStatus.Pending);
        result[0].TableName.Should().Be("Table 1");
    }

    [Fact]
    public async Task GetActiveTicketsAsync_WithErrorResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpKitchenGateway>>();
        var gateway = new HttpKitchenGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.GetActiveTicketsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_WithSuccessResponse_ShouldReturnTrue()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpKitchenGateway>>();
        var gateway = new HttpKitchenGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.UpdateTicketStatusAsync(1, OrderItemStatus.Preparing, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Patch);
        handler.LastRequest.RequestUri!.ToString().Should().EndWith("/kitchen/tickets/1/status");
        var body = await handler.LastRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("\"status\":\"preparing\"");
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_WithErrorResponse_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpKitchenGateway>>();
        var gateway = new HttpKitchenGateway(httpClient, _options, logger.Object);

        // Act
        var result = await gateway.UpdateTicketStatusAsync(1, OrderItemStatus.Preparing, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
