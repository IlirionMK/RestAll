using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Orders;
using Xunit;

namespace RestAll.Desktop.Tests.Orders;

public class ManageOrdersUseCaseOfflineTests
{
    private readonly Mock<IOrderGateway> _mockGateway;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<ManageOrdersUseCase>> _mockLogger;
    private readonly Mock<ISyncManager> _mockSyncManager;
    private readonly Mock<IOfflineStorage> _mockOfflineStorage;
    private readonly ManageOrdersUseCase _useCase;

    public ManageOrdersUseCaseOfflineTests()
    {
        _mockGateway = new Mock<IOrderGateway>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ManageOrdersUseCase>>();
        _mockSyncManager = new Mock<ISyncManager>();
        _mockOfflineStorage = new Mock<IOfflineStorage>();
        
        _useCase = new ManageOrdersUseCase(
            _mockGateway.Object,
            _mockCache.Object,
            _mockLogger.Object,
            _mockSyncManager.Object,
            _mockOfflineStorage.Object
        );
    }

    [Fact]
    public async Task CreateOrderAsync_WhenApiFails_ShouldUseSyncManager()
    {
        // Arrange
        var tableId = 5;
        var tempOrder = new Order(-123456789, tableId, 0, 0m, OrderStatus.Pending, new List<OrderItem>());
        
        _mockGateway
            .Setup(g => g.CreateOrderAsync(tableId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));
        
        _mockSyncManager
            .Setup(s => s.EnqueueCreateOrderAsync(tableId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempOrder);

        // Act
        var result = await _useCase.CreateOrderAsync(tableId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeLessThan(0, "Should return offline order with negative ID");
        _mockSyncManager.Verify(s => s.EnqueueCreateOrderAsync(tableId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddOrderItemsAsync_WithOfflineOrder_ShouldQueueOperation()
    {
        // Arrange
        var localOrderId = -123456789;
        var items = new List<OrderItem>
        {
            new OrderItem(1, 0, 10, "Pizza", 29.99m, 2, "Extra cheese", OrderItemStatus.Pending)
        };
        
        _mockGateway
            .Setup(g => g.AddOrderItemsAsync(localOrderId, items, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _useCase.AddOrderItemsAsync(localOrderId, items, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(localOrderId);
        result.Items.Should().HaveCount(1);
        _mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(localOrderId, items, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveOrderItemAsync_WithOfflineOrder_ShouldQueueOperation()
    {
        // Arrange
        var localOrderId = -123456789;
        var orderItemId = 42;
        
        _mockGateway
            .Setup(g => g.RemoveOrderItemAsync(localOrderId, orderItemId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _useCase.RemoveOrderItemAsync(localOrderId, orderItemId, CancellationToken.None);

        // Assert
        result.Should().BeTrue("Should assume success for offline operations");
        _mockSyncManager.Verify(
            s => s.EnqueueRemoveItemAsync(localOrderId, orderItemId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task PayOrderAsync_WhenApiFails_ShouldQueuePayment()
    {
        // Arrange
        var serverOrderId = 123;
        
        _mockGateway
            .Setup(g => g.PayOrderAsync(serverOrderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Payment gateway timeout"));

        // Act
        var result = await _useCase.PayOrderAsync(serverOrderId, CancellationToken.None);

        // Assert
        result.Should().BeTrue("Should assume payment will succeed when synced");
        _mockSyncManager.Verify(
            s => s.EnqueuePayOrderAsync(serverOrderId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddOrderItemsAsync_WithOnlineOrder_ShouldCallGateway()
    {
        // Arrange
        var onlineOrderId = 123;
        var items = new List<OrderItem>
        {
            new OrderItem(1, 0, 10, "Pizza", 29.99m, 2, "", OrderItemStatus.Pending)
        };
        var updatedOrder = new Order(onlineOrderId, 5, 1, 59.98m, OrderStatus.InProgress, items);
        
        _mockGateway
            .Setup(g => g.AddOrderItemsAsync(onlineOrderId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _useCase.AddOrderItemsAsync(onlineOrderId, items, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(onlineOrderId);
        _mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(It.IsAny<int>(), It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task PayOrderAsync_WithOnlineOrderSuccess_ShouldNotQueue()
    {
        // Arrange
        var serverOrderId = 123;
        
        _mockGateway
            .Setup(g => g.PayOrderAsync(serverOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.PayOrderAsync(serverOrderId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockSyncManager.Verify(
            s => s.EnqueuePayOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
