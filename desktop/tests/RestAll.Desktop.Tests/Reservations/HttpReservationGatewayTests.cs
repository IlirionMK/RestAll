using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Reservations;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Reservations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Reservations;

public class HttpReservationGatewayTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly RestAllApiOptions _options;
    private readonly Mock<ILogger<HttpReservationGateway>> _loggerMock;
    private readonly HttpReservationGateway _gateway;

    public HttpReservationGatewayTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _options = new RestAllApiOptions { BaseUrl = "http://localhost/api" };
        _loggerMock = new Mock<ILogger<HttpReservationGateway>>();
        _gateway = new HttpReservationGateway(_httpClient, _options, _loggerMock.Object);
    }

    [Fact]
    public async Task GetReservationsAsync_ShouldReturnReservations()
    {
        // Arrange
        var date = DateTime.Now;
        var dateString = date.ToString("yyyy-MM-dd");
        var responseJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                id = 1,
                customer_name = "John Doe",
                customer_phone = "123456789",
                customer_email = "john@example.com",
                reservation_date = dateString,
                reservation_time = "18:00",
                table_id = 1,
                number_of_guests = 4,
                status = "confirmed",
                special_requests = "Window seat"
            }
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
        var result = await _gateway.GetReservationsAsync(date, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
        result[0].CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldReturnCreatedReservation()
    {
        // Arrange
        var reservation = new Reservation(
            0,
            "John Doe",
            "123456789",
            "john@example.com",
            DateTime.Now,
            DateTime.Now.AddHours(18),
            1,
            4,
            "confirmed",
            "Window seat"
        );

        var responseJson = JsonSerializer.Serialize(new
        {
            id = 1,
            customer_name = "John Doe",
            customer_phone = "123456789",
            customer_email = "john@example.com",
            reservation_date = DateTime.Now.ToString("yyyy-MM-dd"),
            reservation_time = "18:00",
            table_id = 1,
            number_of_guests = 4,
            status = "confirmed",
            special_requests = "Window seat"
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
        var result = await _gateway.CreateReservationAsync(reservation, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnTrueOnSuccess()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var result = await _gateway.CancelReservationAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnFalseOnFailure()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _gateway.CancelReservationAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
