using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Tables;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Tables;

public class HttpTableGatewayTests
{
    private readonly RestAllApiOptions _options = new RestAllApiOptions { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task GetTablesAsync_WithSuccessResponse_ShouldReturnTables()
    {
        // Arrange
        var responseJson = """
            [
                {
                    "id": 1,
                    "number": "Table 1",
                    "capacity": 4,
                    "status": "available"
                },
                {
                    "id": 2,
                    "number": "Table 2",
                    "capacity": 6,
                    "status": "occupied"
                }
            ]
            """;
        
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
        
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpTableGateway(httpClient, _options);

        // Act
        var result = await gateway.GetTablesAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTablesAsync_WithErrorResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpTableGateway(httpClient, _options);

        // Act
        var result = await gateway.GetTablesAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateTableStatusAsync_WithSuccessResponse_ShouldReturnTrue()
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
        var gateway = new HttpTableGateway(httpClient, _options);

        // Act
        var result = await gateway.UpdateTableStatusAsync(1, TableStatus.Occupied, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTableStatusAsync_WithErrorResponse_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var gateway = new HttpTableGateway(httpClient, _options);

        // Act
        var result = await gateway.UpdateTableStatusAsync(1, TableStatus.Occupied, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
