using Moq;
using RestAll.Desktop.Core.Reservations;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Reservations;

public class ManageReservationsUseCaseTests
{
    private readonly Mock<IReservationGateway> _gatewayMock;
    private readonly ManageReservationsUseCase _useCase;

    public ManageReservationsUseCaseTests()
    {
        _gatewayMock = new Mock<IReservationGateway>();
        _useCase = new ManageReservationsUseCase(_gatewayMock.Object);
    }

    [Fact]
    public async Task GetReservationsForDateAsync_ShouldReturnReservations()
    {
        // Arrange
        var date = DateTime.Now;
        var expectedReservations = new List<Reservation>
        {
            new Reservation(1, "John Doe", "123456789", "john@example.com", date, date.AddHours(18), 1, 4, "confirmed", "Window seat"),
            new Reservation(2, "Jane Smith", "987654321", "jane@example.com", date, date.AddHours(19), 2, 2, "confirmed", null)
        };
        _gatewayMock.Setup(g => g.GetReservationsAsync(date, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedReservations);

        // Act
        var result = await _useCase.GetReservationsForDateAsync(date, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedReservations);
        _gatewayMock.Verify(g => g.GetReservationsAsync(date, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldReturnCreatedReservation()
    {
        // Arrange
        var reservation = new Reservation(
            1,
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
        _gatewayMock.Setup(g => g.CreateReservationAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(reservation);

        // Act
        var result = await _useCase.CreateReservationAsync(
            "John Doe",
            "123456789",
            "john@example.com",
            DateTime.Now,
            DateTime.Now.AddHours(18),
            1,
            4,
            "Window seat",
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(reservation);
        _gatewayMock.Verify(g => g.CreateReservationAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnTrueOnSuccess()
    {
        // Arrange
        _gatewayMock.Setup(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _useCase.CancelReservationAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _gatewayMock.Verify(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldReturnFalseOnFailure()
    {
        // Arrange
        _gatewayMock.Setup(g => g.CancelReservationAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        // Act
        var result = await _useCase.CancelReservationAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
