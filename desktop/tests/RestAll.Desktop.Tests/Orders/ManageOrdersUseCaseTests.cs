using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Orders;
using Xunit;

namespace RestAll.Desktop.Tests.Orders;

public class ManageOrdersUseCaseTests
{
    private readonly Mock<IOrderGateway> _mockGateway;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<ManageOrdersUseCase>> _mockLogger;
    private readonly ManageOrdersUseCase _useCase;

    public ManageOrdersUseCaseTests()
    {
        _mockGateway = new Mock<IOrderGateway>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ManageOrdersUseCase>>();
        _useCase = new ManageOrdersUseCase(_mockGateway.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnOrders()
    {
        // Arrange
        var expectedOrders = new List<Order>
        {
            new Order(1, 1, 1, 25.98m, OrderStatus.Pending, new List<OrderItem>()),
            new Order(2, 2, 1, 15.99m, OrderStatus.InProgress, new List<OrderItem>())
        };

        _mockGateway
            .Setup(g => g.GetOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _useCase.GetOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOrders);
    }

    [Fact]
    public async Task PayOrderAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockGateway
            .Setup(g => g.PayOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.PayOrderAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnOrder()
    {
        // Arrange
        var expectedOrder = new Order(1, 1, 1, 0m, OrderStatus.Pending, new List<OrderItem>());

        _mockGateway
            .Setup(g => g.CreateOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _useCase.CreateOrderAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOrder);
    }
}
