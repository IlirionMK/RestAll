using Moq;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Cache;
using FluentAssertions;
using Xunit;

namespace RestAll.Desktop.Tests.Integration;

public class OrdersIntegrationTests
{
    private readonly Mock<IOrderGateway> _gatewayMock;
    private readonly Mock<IAuthenticateUserUseCase> _authUseCaseMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<ManageOrdersUseCase>> _loggerMock;
    private readonly ManageOrdersUseCase _useCase;

    public OrdersIntegrationTests()
    {
        _gatewayMock = new Mock<IOrderGateway>();
        _authUseCaseMock = new Mock<IAuthenticateUserUseCase>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ManageOrdersUseCase>>();
        _useCase = new ManageOrdersUseCase(_gatewayMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task FullOrderFlow_ShouldWork()
    {
        // Arrange
        var accessToken = "test_token";
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(accessToken, "refresh_token", "John Doe", "waiter"));

        var menuItem = new OrderItem(1, 1, 101, "Burger", 25.00m, 2, null, OrderItemStatus.Pending);
        var newOrder = new Order(1, 1, 1, 50.00m, OrderStatus.Pending, new List<OrderItem> { menuItem });

        _gatewayMock.Setup(g => g.CreateOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newOrder);

        _gatewayMock.Setup(g => g.GetOrdersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Order> { newOrder });

        // Act - Create order
        var createdOrder = await _useCase.CreateOrderAsync(1, CancellationToken.None);

        // Assert - Order created
        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().Be(1);
        createdOrder.TableId.Should().Be(1);
        createdOrder.Status.Should().Be(OrderStatus.Pending);
        createdOrder.Items.Should().HaveCount(1);

        // Act - Get orders
        var orders = await _useCase.GetOrdersAsync(CancellationToken.None);

        // Assert - Orders retrieved
        orders.Should().NotBeNull();
        orders.Should().HaveCount(1);
        orders[0].Id.Should().Be(1);

        // Arrange - Add item
        var additionalItem = new OrderItem(2, 1, 102, "Fries", 5.00m, 1, null, OrderItemStatus.Pending);
        var updatedOrderWithItem = new Order(1, 1, 1, 55.00m, OrderStatus.Pending, new List<OrderItem> { menuItem, additionalItem });

        _gatewayMock.Setup(g => g.AddOrderItemsAsync(1, It.Is<List<OrderItem>>(items => items.Count == 1 && items[0].MenuItemId == 102), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedOrderWithItem);

        // Act - Add item
        var orderWithItem = await _useCase.AddOrderItemsAsync(1, new List<OrderItem> { additionalItem }, CancellationToken.None);

        // Assert - Item added
        orderWithItem.Should().NotBeNull();
        orderWithItem!.Items.Should().HaveCount(2);
        orderWithItem.TotalAmount.Should().Be(55.00m);

        // Arrange - Remove item
        var orderAfterRemoval = new Order(1, 1, 1, 50.00m, OrderStatus.Pending, new List<OrderItem> { menuItem });

        _gatewayMock.Setup(g => g.RemoveOrderItemAsync(1, 2, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        _gatewayMock.Setup(g => g.GetOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orderAfterRemoval);

        // Act - Remove item
        var removed = await _useCase.RemoveOrderItemAsync(1, 2, CancellationToken.None);

        // Assert - Item removed
        removed.Should().BeTrue();

        // Verify gateway calls
        _gatewayMock.Verify(g => g.CreateOrderAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(g => g.AddOrderItemsAsync(1, It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _gatewayMock.Verify(g => g.RemoveOrderItemAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OrderFlow_WithComments_ShouldWork()
    {
        // Arrange
        var accessToken = "test_token";
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(accessToken, "refresh_token", "John Doe", "waiter"));

        var menuItem = new OrderItem(1, 1, 101, "Burger", 25.00m, 1, "No onions", OrderItemStatus.Pending);
        var newOrder = new Order(1, 1, 1, 25.00m, OrderStatus.Pending, new List<OrderItem> { menuItem });

        _gatewayMock.Setup(g => g.CreateOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newOrder);

        _gatewayMock.Setup(g => g.AddOrderItemsAsync(1, It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newOrder);

        // Act - Create order with comment
        var createdOrder = await _useCase.CreateOrderAsync(1, CancellationToken.None);

        // Assert
        createdOrder.Should().NotBeNull();
        createdOrder!.Items[0].Comment.Should().Be("No onions");

        // Act - Add item with comment
        var orderWithComment = await _useCase.AddOrderItemsAsync(1, new List<OrderItem> { menuItem }, CancellationToken.None);

        // Assert
        orderWithComment.Should().NotBeNull();
        orderWithComment!.Items[0].Comment.Should().Be("No onions");
    }

    [Fact]
    public async Task OrderFlow_WithMultipleItems_ShouldWork()
    {
        // Arrange
        var accessToken = "test_token";
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(accessToken, "refresh_token", "John Doe", "waiter"));

        var item1 = new OrderItem(1, 1, 101, "Burger", 25.00m, 2, null, OrderItemStatus.Pending);
        var item2 = new OrderItem(2, 1, 102, "Fries", 5.00m, 2, null, OrderItemStatus.Pending);
        var item3 = new OrderItem(3, 1, 103, "Drink", 5.00m, 2, null, OrderItemStatus.Pending);

        var orderWithMultipleItems = new Order(1, 1, 1, 70.00m, OrderStatus.Pending, new List<OrderItem> { item1, item2, item3 });

        _gatewayMock.Setup(g => g.CreateOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(orderWithMultipleItems);

        _gatewayMock.Setup(g => g.GetOrdersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Order> { orderWithMultipleItems });

        // Act - Create order
        var createdOrder = await _useCase.CreateOrderAsync(1, CancellationToken.None);

        // Assert
        createdOrder.Should().NotBeNull();
        createdOrder!.Items.Should().HaveCount(3);
        createdOrder.TotalAmount.Should().Be(70.00m);

        // Act - Get orders
        var orders = await _useCase.GetOrdersAsync(CancellationToken.None);

        // Assert
        orders.Should().HaveCount(1);
        orders[0].Items.Should().HaveCount(3);
        orders[0].TotalAmount.Should().Be(70.00m);
    }

    [Fact]
    public async Task OrderFlow_WithPayment_ShouldWork()
    {
        // Arrange
        var accessToken = "test_token";
        _authUseCaseMock.Setup(a => a.CurrentSession).Returns(new UserSession(accessToken, "refresh_token", "John Doe", "waiter"));

        var menuItem = new OrderItem(1, 1, 101, "Burger", 25.00m, 2, null, OrderItemStatus.Pending);
        var pendingOrder = new Order(1, 1, 1, 50.00m, OrderStatus.Pending, new List<OrderItem> { menuItem });
        var paidOrder = new Order(1, 1, 1, 50.00m, OrderStatus.Paid, new List<OrderItem> { menuItem });

        _gatewayMock.Setup(g => g.CreateOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(pendingOrder);

        _gatewayMock.Setup(g => g.PayOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        _gatewayMock.Setup(g => g.GetOrderAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(paidOrder);

        _gatewayMock.Setup(g => g.GetOrdersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Order> { paidOrder });

        // Act - Create order
        var createdOrder = await _useCase.CreateOrderAsync(1, CancellationToken.None);

        // Assert - Order is pending
        createdOrder!.Status.Should().Be(OrderStatus.Pending);

        // Act - Pay for order
        var paid = await _useCase.PayOrderAsync(1, CancellationToken.None);

        // Assert - Payment successful
        paid.Should().BeTrue();

        // Act - Get orders
        var orders = await _useCase.GetOrdersAsync(CancellationToken.None);

        // Assert - Order status is updated
        orders[0].Status.Should().Be(OrderStatus.Paid);
    }
}
