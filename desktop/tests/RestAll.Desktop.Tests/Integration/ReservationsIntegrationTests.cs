using Moq;
using RestAll.Desktop.Core.Reservations;
using RestAll.Desktop.Core.Auth;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

public class ReservationsIntegrationTests
{
    private readonly Mock<IReservationGateway> _gatewayMock;
    private readonly Mock<IAuthenticateUserUseCase> _authUseCaseMock;
    private readonly ManageReservationsUseCase _useCase;

    public ReservationsIntegrationTests()
    {
        _gatewayMock = new Mock<IReservationGateway>();
        _authUseCaseMock = new Mock<IAuthenticateUserUseCase>();
        _useCase = new ManageReservationsUseCase(_gatewayMock.Object);
    }

    [Fact]
    public async Task FullReservationFlow_ShouldWork()
    {
        // Arrange
        var date = DateTime.Now;
        var accessToken = "test_token";
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(accessToken, "refresh_token", "John Doe", "waiter"));

        var newReservation = new Reservation(
            1,
            "John Doe",
            "123456789",
            "john@example.com",
            date,
            date.AddHours(18),
            1,
            4,
            "confirmed",
            "Window seat"
        );

        _gatewayMock.Setup(g => g.CreateReservationAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newReservation);

        _gatewayMock.Setup(g => g.GetReservationsAsync(date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Reservation> { newReservation });

        // Act - Create reservation
        var createdReservation = await _useCase.CreateReservationAsync(
            "John Doe",
            "123456789",
            "john@example.com",
            date,
            date.AddHours(18),
            1,
            4,
            "Window seat",
            CancellationToken.None
        );

        // Assert - Reservation created
        createdReservation.Should().NotBeNull();
        createdReservation!.Id.Should().Be(1);
        createdReservation.CustomerName.Should().Be("John Doe");

        // Act - Get reservations for the date
        var reservations = await _useCase.GetReservationsForDateAsync(date, CancellationToken.None);

        // Assert - Reservations retrieved
        reservations.Should().NotBeNull();
        reservations.Should().HaveCount(1);
        reservations[0].Id.Should().Be(1);

        // Arrange - Cancel reservation mock
        _gatewayMock.Setup(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act - Cancel reservation
        var cancelled = await _useCase.CancelReservationAsync(1, CancellationToken.None);

        // Assert - Reservation cancelled
        cancelled.Should().BeTrue();
        _gatewayMock.Verify(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReservationsAfterCancellation_ShouldReturnUpdatedList()
    {
        // Arrange
        var date = DateTime.Now;
        var reservations = new List<Reservation>
        {
            new Reservation(1, "John Doe", "123456789", "john@example.com", date, date.AddHours(18), 1, 4, "confirmed", null),
            new Reservation(2, "Jane Smith", "987654321", "jane@example.com", date, date.AddHours(19), 2, 2, "confirmed", null)
        };

        _gatewayMock.Setup(g => g.GetReservationsAsync(date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(reservations);

        _gatewayMock.Setup(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act - Get initial reservations
        var initialReservations = await _useCase.GetReservationsForDateAsync(date, CancellationToken.None);
        initialReservations.Should().HaveCount(2);

        // Act - Cancel one reservation
        await _useCase.CancelReservationAsync(1, CancellationToken.None);

        // Act - Get updated reservations
        var updatedReservations = new List<Reservation>
        {
            new Reservation(2, "Jane Smith", "987654321", "jane@example.com", date, date.AddHours(19), 2, 2, "confirmed", null)
        };
        _gatewayMock.Setup(g => g.GetReservationsAsync(date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedReservations);

        var finalReservations = await _useCase.GetReservationsForDateAsync(date, CancellationToken.None);

        // Assert
        finalReservations.Should().HaveCount(1);
        finalReservations[0].Id.Should().Be(2);
    }
}
