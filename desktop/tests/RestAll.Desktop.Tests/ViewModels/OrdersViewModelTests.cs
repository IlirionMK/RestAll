using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class OrdersViewModelTests
{
    private readonly Mock<IManageOrdersUseCase> _mockUseCase;
    private readonly Mock<IRealtimeService> _mockRealtime;
    private readonly OrdersViewModel _viewModel;

    public OrdersViewModelTests()
    {
        _mockUseCase = new Mock<IManageOrdersUseCase>();
        _mockRealtime = new Mock<IRealtimeService>();
        _viewModel = new OrdersViewModel(_mockUseCase.Object, _mockRealtime.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Orders.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("");
        _viewModel.LoadOrdersCommand.Should().NotBeNull();
        _viewModel.PayOrderCommand.Should().NotBeNull();
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_ShouldDisableCommands()
    {
        // Arrange
        _viewModel.LoadOrdersCommand.CanExecute(null).Should().BeTrue();

        // Act
        _viewModel.IsLoading = true;

        // Assert
        _viewModel.LoadOrdersCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoadOrdersCommand_ShouldLoadOrders()
    {
        // Arrange
        var expectedOrders = new List<Order>
        {
            new Order(1, 1, 1, 25.98m, OrderStatus.Pending, new List<OrderItem>()),
            new Order(2, 2, 1, 15.99m, OrderStatus.InProgress, new List<OrderItem>())
        };

        _mockUseCase
            .Setup(u => u.GetOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        _viewModel.LoadOrdersCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.Orders.Should().BeEquivalentTo(expectedOrders);
        _viewModel.StatusMessage.Should().Contain("2 orders");
    }

    [Fact]
    public async Task LoadOrdersCommand_WhenError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.GetOrdersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        _viewModel.LoadOrdersCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Error loading orders");
        _viewModel.StatusMessage.Should().Contain("Network error");
    }

    [Fact]
    public async Task PayOrderCommand_WhenSuccess_ShouldCallUseCase()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.PayOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUseCase
            .Setup(u => u.GetOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        _viewModel.PayOrderCommand.Execute(1);
        await Task.Delay(200);

        // Assert
        _mockUseCase.Verify(u => u.PayOrderAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayOrderCommand_WhenFailure_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.PayOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        _viewModel.PayOrderCommand.Execute(1);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Failed to pay order 1");
    }

    [Fact]
    public void Cancel_ShouldCancelCancellationToken()
    {
        // Arrange
        // Act
        _viewModel.Cancel();

        // Assert
        // Should not throw exception
    }
}
