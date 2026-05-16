using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class MenuViewModelTests
{
    private readonly Mock<IGetMenuUseCase> _mockUseCase;
    private readonly Mock<IOfflineStorage> _mockOfflineStorage;
    private readonly MenuViewModel _viewModel;

    public MenuViewModelTests()
    {
        _mockUseCase = new Mock<IGetMenuUseCase>();
        _mockOfflineStorage = new Mock<IOfflineStorage>();
        _viewModel = new MenuViewModel(_mockUseCase.Object, _mockOfflineStorage.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Categories.Should().BeEmpty();
        _viewModel.Items.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("");
        _viewModel.LoadMenuCommand.Should().NotBeNull();
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_ShouldDisableCommand()
    {
        // Arrange
        var command = _viewModel.LoadMenuCommand;
        command.CanExecute(null).Should().BeTrue();

        // Act
        _viewModel.IsLoading = true;

        // Assert
        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void IsLoading_WhenSetToFalse_ShouldEnableCommand()
    {
        // Arrange
        _viewModel.IsLoading = true;
        var command = _viewModel.LoadMenuCommand;
        command.CanExecute(null).Should().BeFalse();

        // Act
        _viewModel.IsLoading = false;

        // Assert
        command.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task LoadMenuCommand_ShouldLoadCategoriesAndItems()
    {
        // Arrange
        var expectedCategories = new List<MenuCategory>
        {
            new MenuCategory(1, "Appetizers", 1, new List<MenuItem>())
        };
        var expectedItems = new List<MenuItem>
        {
            new MenuItem(1, "Burger", "Delicious", 15.99m, null, true, 1, "Appetizers")
        };

        _mockUseCase
            .Setup(u => u.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategories);
        _mockUseCase
            .Setup(u => u.GetItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        _viewModel.LoadMenuCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.Categories.Should().BeEquivalentTo(expectedCategories);
        _viewModel.Items.Should().BeEquivalentTo(expectedItems);
        _viewModel.StatusMessage.Should().Contain("1 categories");
        _viewModel.StatusMessage.Should().Contain("1 items");
    }

    [Fact]
    public async Task LoadMenuCommand_WhenError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        _viewModel.LoadMenuCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Error loading menu");
        _viewModel.StatusMessage.Should().Contain("Network error");
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
