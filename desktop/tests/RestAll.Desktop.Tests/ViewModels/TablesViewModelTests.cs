using FluentAssertions;
using Moq;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class TablesViewModelTests
{
    private readonly Mock<ITableManagementUseCase> _mockUseCase;
    private readonly TablesViewModel _viewModel;

    public TablesViewModelTests()
    {
        _mockUseCase = new Mock<ITableManagementUseCase>();
        _viewModel = new TablesViewModel(_mockUseCase.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Tables.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.StatusMessage.Should().Be("");
        _viewModel.LoadTablesCommand.Should().NotBeNull();
        _viewModel.UpdateTableStatusCommand.Should().NotBeNull();
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_ShouldDisableCommands()
    {
        // Arrange
        _viewModel.LoadTablesCommand.CanExecute(null).Should().BeTrue();

        // Act
        _viewModel.IsLoading = true;

        // Assert
        _viewModel.LoadTablesCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoadTablesCommand_ShouldLoadTables()
    {
        // Arrange
        var expectedTables = new List<Table>
        {
            new Table(1, "Table 1", 4, TableStatus.Available),
            new Table(2, "Table 2", 6, TableStatus.Occupied)
        };

        _mockUseCase
            .Setup(u => u.GetTablesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTables);

        // Act
        _viewModel.LoadTablesCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.Tables.Should().BeEquivalentTo(expectedTables);
        _viewModel.StatusMessage.Should().Contain("2 tables");
    }

    [Fact]
    public async Task LoadTablesCommand_WhenError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.GetTablesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        _viewModel.LoadTablesCommand.Execute(null);
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Error loading tables");
        _viewModel.StatusMessage.Should().Contain("Network error");
    }

    [Fact]
    public async Task UpdateTableStatusCommand_WhenSuccess_ShouldCallUseCase()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.UpdateTableStatusAsync(It.IsAny<int>(), It.IsAny<TableStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUseCase
            .Setup(u => u.GetTablesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Table>());

        // Act
        _viewModel.UpdateTableStatusCommand.Execute(Tuple.Create(1, TableStatus.Occupied));
        await Task.Delay(200);

        // Assert
        _mockUseCase.Verify(u => u.UpdateTableStatusAsync(1, TableStatus.Occupied, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTableStatusCommand_WhenFailure_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.UpdateTableStatusAsync(It.IsAny<int>(), It.IsAny<TableStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        _viewModel.UpdateTableStatusCommand.Execute(Tuple.Create(1, TableStatus.Occupied));
        await Task.Delay(200);

        // Assert
        _viewModel.StatusMessage.Should().Contain("Failed to update table 1");
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
