using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Admin;
using RestAll.Desktop.Infrastructure.Admin;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Tests.TestHelpers;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Admin;

public class HttpAdminGatewayTests
{
    private readonly RestAllApiOptions _options = new() { BaseUrl = "http://localhost:8000" };

    [Fact]
    public async Task GetStaffAsync_WithSuccessResponse_ShouldReturnUsers()
    {
        var responseJson = """
            [
                {
                    "id": 1,
                    "name": "Anna Admin",
                    "email": "anna@example.com",
                    "role": "admin",
                    "restaurant_id": 7
                },
                {
                    "id": 2,
                    "name": "Tom Waiter",
                    "email": "tom@example.com",
                    "role": "waiter"
                }
            ]
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpAdminGateway>>();
        var gateway = new HttpAdminGateway(httpClient, _options, logger.Object);

        var result = await gateway.GetStaffAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Anna Admin");
        result[0].RestaurantId.Should().Be(7);
        result[1].Role.Should().Be("waiter");
    }

    [Fact]
    public async Task GetAnalyticsSummaryAsync_WithSuccessResponse_ShouldReturnSummary()
    {
        var responseJson = """
            {
                "revenue": {
                    "today": 100.25,
                    "this_week": 550.5,
                    "this_month": 2200.75
                },
                "orders": {
                    "today": 4,
                    "this_week": 21,
                    "this_month": 88,
                    "average_value": 26.75
                },
                "top_items": [
                    { "name": "Burger", "quantity_sold": 18, "revenue": 215.5 },
                    { "name": "Fries", "quantity_sold": 12, "revenue": 72 }
                ],
                "reservations": {
                    "today": 3,
                    "this_week": 11
                }
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpAdminGateway>>();
        var gateway = new HttpAdminGateway(httpClient, _options, logger.Object);

        var result = await gateway.GetAnalyticsSummaryAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result!.Revenue.Today.Should().Be(100.25m);
        result.Orders.AverageValue.Should().Be(26.75m);
        result.TopItems.Should().HaveCount(2);
        result.TopItems[0].Name.Should().Be("Burger");
        result.Reservations.Today.Should().Be(3);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithSuccessResponse_ShouldReturnPaginatedLogs()
    {
        var responseJson = """
            {
                "data": [
                    {
                        "id": 10,
                        "user_id": 1,
                        "user": { "name": "Anna Admin" },
                        "action": "updated_role",
                        "model_type": "User",
                        "model_id": 2,
                        "payload": { "role": "chef" },
                        "ip_address": "127.0.0.1",
                        "created_at": "2026-05-15T10:12:30Z"
                    }
                ],
                "current_page": 1,
                "last_page": 2,
                "total": 35,
                "per_page": 25
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var logger = new Mock<ILogger<HttpAdminGateway>>();
        var gateway = new HttpAdminGateway(httpClient, _options, logger.Object);

        var result = await gateway.GetAuditLogsAsync(new AuditLogQuery(PerPage: 25), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Action.Should().Be("updated_role");
        result.Items[0].UserName.Should().Be("Anna Admin");
        result.CurrentPage.Should().Be(1);
        result.LastPage.Should().Be(2);
        result.Total.Should().Be(35);
    }
}

