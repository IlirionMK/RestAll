using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Orders;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

/// <summary>
/// End-to-end tests for complete offline order workflow
/// </summary>
public class OfflineOrderWorkflowTests
{
    [Fact]
    public async Task CompleteOfflineOrderWorkflow_ShouldQueueAllOperations()
    {
        // Arrange - Setup mocks
        var mockGateway = new Mock<IOrderGateway>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<ManageOrdersUseCase>>();
        var mockSyncManager = new Mock<ISyncManager>();
        var mockOfflineStorage = new Mock<IOfflineStorage>();
        
        var useCase = new ManageOrdersUseCase(
            mockGateway.Object,
            mockCache.Object,
            mockLogger.Object,
            mockSyncManager.Object,
            mockOfflineStorage.Object
        );

        var tableId = 5;
        var tempOrderId = -123456789;
        var tempOrder = new Order(tempOrderId, tableId, 0, 0m, OrderStatus.Pending, new List<OrderItem>());
        
        // Simulate API failure for create order
        mockGateway
            .Setup(g => g.CreateOrderAsync(tableId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network unavailable"));
        
        mockSyncManager
            .Setup(s => s.EnqueueCreateOrderAsync(tableId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempOrder);

        // Act - Step 1: Create order offline
        var createdOrder = await useCase.CreateOrderAsync(tableId, CancellationToken.None);

        // Assert - Step 1
        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().BeLessThan(0, "Should be offline order");

        // Arrange - Step 2: Add items to offline order
        var items = new List<OrderItem>
        {
            new OrderItem(1, 0, 10, "Pizza Margherita", 29.99m, 2, "Extra cheese", OrderItemStatus.Pending),
            new OrderItem(2, 0, 11, "Pasta Carbonara", 24.99m, 1, "", OrderItemStatus.Pending)
        };

        mockGateway
            .Setup(g => g.AddOrderItemsAsync(tempOrderId, items, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Still offline"));

        // Act - Step 2: Add items (will fail and queue)
        var updatedOrder = await useCase.AddOrderItemsAsync(tempOrderId, items, CancellationToken.None);

        // Assert - Step 2
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Items.Should().HaveCount(2);
        mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(tempOrderId, items, It.IsAny<CancellationToken>()),
            Times.Once,
            "AddItems operation should be queued"
        );

        // Arrange - Step 3: Remove one item
        var orderItemIdToRemove = 1;
        mockGateway
            .Setup(g => g.RemoveOrderItemAsync(tempOrderId, orderItemIdToRemove, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Still offline"));

        // Act - Step 3: Remove item (will fail and queue)
        var removeResult = await useCase.RemoveOrderItemAsync(tempOrderId, orderItemIdToRemove, CancellationToken.None);

        // Assert - Step 3
        removeResult.Should().BeTrue("Should assume success for offline operations");
        mockSyncManager.Verify(
            s => s.EnqueueRemoveItemAsync(tempOrderId, orderItemIdToRemove, It.IsAny<CancellationToken>()),
            Times.Once,
            "RemoveItem operation should be queued"
        );

        // Verify total operations queued
        mockSyncManager.Verify(s => s.EnqueueCreateOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        mockSyncManager.Verify(s => s.EnqueueAddItemsAsync(It.IsAny<int>(), It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        mockSyncManager.Verify(s => s.EnqueueRemoveItemAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnlineOrderWorkflow_ShouldNotQueueOperations()
    {
        // Arrange
        var mockGateway = new Mock<IOrderGateway>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<ManageOrdersUseCase>>();
        var mockSyncManager = new Mock<ISyncManager>();
        var mockOfflineStorage = new Mock<IOfflineStorage>();
        
        var useCase = new ManageOrdersUseCase(
            mockGateway.Object,
            mockCache.Object,
            mockLogger.Object,
            mockSyncManager.Object,
            mockOfflineStorage.Object
        );

        var onlineOrderId = 123;
        var items = new List<OrderItem>
        {
            new OrderItem(1, 0, 10, "Pizza", 29.99m, 2, "", OrderItemStatus.Pending)
        };
        var updatedOrder = new Order(onlineOrderId, 5, 1, 59.98m, OrderStatus.InProgress, items);

        // All API calls succeed
        mockGateway
            .Setup(g => g.AddOrderItemsAsync(onlineOrderId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedOrder);

        mockGateway
            .Setup(g => g.PayOrderAsync(onlineOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act - Add items (should succeed without queuing)
        var addResult = await useCase.AddOrderItemsAsync(onlineOrderId, items, CancellationToken.None);

        // Act - Pay order (should succeed without queuing)
        var payResult = await useCase.PayOrderAsync(onlineOrderId, CancellationToken.None);

        // Assert - No operations should be queued
        addResult.Should().NotBeNull();
        payResult.Should().BeTrue();
        
        mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(It.IsAny<int>(), It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Online orders should not queue operations"
        );
        
        mockSyncManager.Verify(
            s => s.EnqueuePayOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Successful payments should not be queued"
        );
    }

    [Fact]
    public async Task MixedWorkflow_OnlineAndOffline_ShouldQueueOnlyOffline()
    {
        // Arrange
        var mockGateway = new Mock<IOrderGateway>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<ManageOrdersUseCase>>();
        var mockSyncManager = new Mock<ISyncManager>();
        var mockOfflineStorage = new Mock<IOfflineStorage>();
        
        var useCase = new ManageOrdersUseCase(
            mockGateway.Object,
            mockCache.Object,
            mockLogger.Object,
            mockSyncManager.Object,
            mockOfflineStorage.Object
        );

        var onlineOrderId = 100;
        var offlineOrderId = -987654321;
        var items = new List<OrderItem>
        {
            new OrderItem(1, 0, 10, "Pizza", 29.99m, 1, "", OrderItemStatus.Pending)
        };

        // Online order succeeds
        mockGateway
            .Setup(g => g.AddOrderItemsAsync(onlineOrderId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order(onlineOrderId, 5, 1, 29.99m, OrderStatus.Pending, items));

        // Offline order fails
        mockGateway
            .Setup(g => g.AddOrderItemsAsync(offlineOrderId, items, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Offline"));

        // Act
        await useCase.AddOrderItemsAsync(onlineOrderId, items, CancellationToken.None);
        await useCase.AddOrderItemsAsync(offlineOrderId, items, CancellationToken.None);

        // Assert - Only offline order should be queued
        mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(offlineOrderId, items, It.IsAny<CancellationToken>()),
            Times.Once,
            "Only offline order should be queued"
        );
        
        mockSyncManager.Verify(
            s => s.EnqueueAddItemsAsync(onlineOrderId, It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Online order should not be queued"
        );
    }
}
