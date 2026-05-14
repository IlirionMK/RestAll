using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class KitchenViewModelTests
{
    private readonly Mock<IManageKitchenUseCase> _mockUseCase;
    private readonly Mock<IRealtimeService> _mockRealtime;
    private readonly KitchenViewModel _viewModel;

    public KitchenViewModelTests()
    {
        _mockUseCase = new Mock<IManageKitchenUseCase>();
        _mockRealtime = new Mock<IRealtimeService>();
        _viewModel = new KitchenViewModel(_mockUseCase.Object, _mockRealtime.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Tickets.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("");
        _viewModel.LoadTicketsCommand.Should().NotBeNull();
        _viewModel.UpdateTicketStatusCommand.Should().NotBeNull();
        _viewModel.SetReadyCommand.Should().NotBeNull();
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_ShouldDisableCommands()
    {
        // Arrange
        _viewModel.LoadTicketsCommand.CanExecute(null).Should().BeTrue();

        // Act
        _viewModel.IsLoading = true;

        // Assert
        _viewModel.LoadTicketsCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoadTicketsCommand_ShouldLoadTickets()
    {
        // Arrange
        var expectedTickets = new List<KitchenTicket>
        {
            new KitchenTicket(1, 1, 1, "Burger", 15.99m, 2, null, OrderItemStatus.Pending, "Table 1"),
            new KitchenTicket(2, 1, 2, "Fries", 5.99m, 1, null, OrderItemStatus.Preparing, "Table 1")
        };

        _mockUseCase
            .Setup(u => u.GetActiveTicketsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTickets);

        // Act
        _viewModel.LoadTicketsCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.Tickets.Should().BeEquivalentTo(expectedTickets);
        _viewModel.StatusMessage.Should().Contain("2 active tickets");
    }

    [Fact]
    public async Task LoadTicketsCommand_WhenError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.GetActiveTicketsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        _viewModel.LoadTicketsCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Error loading tickets");
        _viewModel.StatusMessage.Should().Contain("Network error");
    }

    [Fact]
    public async Task UpdateTicketStatusCommand_WhenSuccess_ShouldCallUseCase()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.UpdateTicketStatusAsync(It.IsAny<int>(), It.IsAny<OrderItemStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUseCase
            .Setup(u => u.GetActiveTicketsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KitchenTicket>());

        // Act
        _viewModel.UpdateTicketStatusCommand.Execute(1);
        await Task.Delay(200);

        // Assert
        _mockUseCase.Verify(u => u.UpdateTicketStatusAsync(1, OrderItemStatus.Preparing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetReadyCommand_ShouldCallUseCase()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.UpdateTicketStatusAsync(It.IsAny<int>(), It.IsAny<OrderItemStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUseCase
            .Setup(u => u.GetActiveTicketsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KitchenTicket>());

        // Act
        _viewModel.SetReadyCommand.Execute(1);
        await Task.Delay(200);

        // Assert
        _mockUseCase.Verify(u => u.UpdateTicketStatusAsync(1, OrderItemStatus.Ready, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketStatusCommand_WhenFailure_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.UpdateTicketStatusAsync(It.IsAny<int>(), It.IsAny<OrderItemStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        _viewModel.UpdateTicketStatusCommand.Execute(1);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Failed to update ticket 1");
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
