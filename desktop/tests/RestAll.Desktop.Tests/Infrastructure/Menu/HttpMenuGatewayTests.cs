using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Menu;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Menu;

public class HttpMenuGatewayTests
{
    private readonly RestAllApiOptions _options = new RestAllApiOptions { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task GetCategoriesAsync_WithSuccessResponse_ShouldReturnCategories()
    {
        // Arrange
        var responseJson = """
            [
                {
                    "id": 1,
                    "name": "Appetizers",
                    "sort_order": 1,
                    "items": []
                },
                {
                    "id": 2,
                    "name": "Main Course",
                    "sort_order": 2,
                    "items": []
                }
            ]
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpMenuGateway(httpClient, _options);

        // Act
        var result = await gateway.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Appetizers");
        result[1].Id.Should().Be(2);
        result[1].Name.Should().Be("Main Course");
    }

    [Fact]
    public async Task GetCategoriesAsync_WithErrorResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpMenuGateway(httpClient, _options);

        // Act
        var result = await gateway.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoriesAsync_WithNetworkError_ShouldReturnEmptyList()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");
        var handler = new MockHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpMenuGateway(httpClient, _options);

        // Act
        var result = await gateway.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetItemsAsync_WithSuccessResponse_ShouldReturnItems()
    {
        // Arrange
        var responseJson = """
            [
                {
                    "id": 1,
                    "name": "Burger",
                    "description": "Delicious burger",
                    "price": 15.99,
                    "photo_url": null,
                    "is_available": true,
                    "menu_category_id": 1,
                    "category": {
                        "name": "Main Course"
                    }
                },
                {
                    "id": 2,
                    "name": "Fries",
                    "description": "Crispy fries",
                    "price": 5.99,
                    "photo_url": null,
                    "is_available": true,
                    "menu_category_id": 1,
                    "category": {
                        "name": "Appetizers"
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
        var gateway = new HttpMenuGateway(httpClient, _options);

        // Act
        var result = await gateway.GetItemsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Burger");
        result[0].Price.Should().Be(15.99m);
        result[1].Id.Should().Be(2);
        result[1].Name.Should().Be("Fries");
    }

    [Fact]
    public async Task GetItemsAsync_WithErrorResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpMenuGateway(httpClient, _options);

        // Act
        var result = await gateway.GetItemsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
