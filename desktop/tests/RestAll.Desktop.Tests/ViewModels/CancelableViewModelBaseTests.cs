using FluentAssertions;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class CancelableViewModelBaseTests
{
    private class TestCancelableViewModel : CancelableViewModelBase
    {
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var viewModel = new TestCancelableViewModel();

        // Assert
        viewModel.IsLoading.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("");
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.IsLoading = true;

        // Assert
        propertyChangedRaised.Should().BeTrue();
        viewModel.IsLoading.Should().BeTrue();
    }

    [Fact]
    public void StatusMessage_WhenSet_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.StatusMessage = "Test message";

        // Assert
        propertyChangedRaised.Should().BeTrue();
        viewModel.StatusMessage.Should().Be("Test message");
    }

    [Fact]
    public void GetCancellationToken_ShouldReturnNewCancellationToken()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();

        // Act
        var cts1 = viewModel.GetCancellationToken();
        var cts2 = viewModel.GetCancellationToken();

        // Assert
        cts1.Should().NotBeNull();
        cts2.Should().NotBeNull();
        cts1.Should().NotBeSameAs(cts2);
    }

    [Fact]
    public void GetCancellationToken_ShouldCancelPreviousToken()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        var cts1 = viewModel.GetCancellationToken();
        var cancelled = false;
        cts1.Token.Register(() => cancelled = true);

        // Act
        viewModel.GetCancellationToken();

        // Assert
        cancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldCancelCancellationToken()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        var cts = viewModel.GetCancellationToken();
        var cancelled = false;
        cts.Token.Register(() => cancelled = true);

        // Act
        viewModel.Cancel();

        // Assert
        cancelled.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldCancelCancellationToken()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        var cts = viewModel.GetCancellationToken();
        var cancelled = false;
        cts.Token.Register(() => cancelled = true);

        // Act
        viewModel.Dispose();

        // Assert
        cancelled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithCancellationAsync_WhenActionThrows_ShouldSetStatusMessage()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();

        // Act
        await viewModel.ExecuteWithCancellationAsync(async ct =>
        {
            await Task.Delay(10, ct);
            throw new Exception("Test error");
        }, "Test operation");

        // Assert
        viewModel.StatusMessage.Should().Contain("Test operation");
        viewModel.StatusMessage.Should().Contain("Test error");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWithCancellationAsync_ShouldSetIsLoadingFalseInFinally()
    {
        // Arrange
        var viewModel = new TestCancelableViewModel();
        viewModel.IsLoading = true;

        // Act
        await viewModel.ExecuteWithCancellationAsync(async ct =>
        {
            await Task.Delay(10, ct);
        }, "Test operation");


        // Assert
        viewModel.IsLoading.Should().BeFalse();
    }
}
