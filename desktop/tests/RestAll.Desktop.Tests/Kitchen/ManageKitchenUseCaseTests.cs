using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Orders;
using Xunit;

namespace RestAll.Desktop.Tests.Kitchen;

public class ManageKitchenUseCaseTests
{
    private readonly Mock<IKitchenGateway> _mockGateway;
    private readonly ManageKitchenUseCase _useCase;

    public ManageKitchenUseCaseTests()
    {
        _mockGateway = new Mock<IKitchenGateway>();
        _useCase = new ManageKitchenUseCase(_mockGateway.Object);
    }

    [Fact]
    public async Task GetActiveTicketsAsync_ShouldReturnTickets()
    {
        // Arrange
        var expectedTickets = new List<KitchenTicket>
        {
            new KitchenTicket(1, 1, 1, "Burger", 15.99m, 2, null, OrderItemStatus.Pending, "Table 1"),
            new KitchenTicket(2, 1, 2, "Fries", 5.99m, 1, null, OrderItemStatus.Preparing, "Table 1")
        };

        _mockGateway
            .Setup(g => g.GetActiveTicketsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTickets);

        // Act
        var result = await _useCase.GetActiveTicketsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedTickets);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockGateway
            .Setup(g => g.UpdateTicketStatusAsync(It.IsAny<int>(), It.IsAny<OrderItemStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.UpdateTicketStatusAsync(1, OrderItemStatus.Preparing, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }
}
