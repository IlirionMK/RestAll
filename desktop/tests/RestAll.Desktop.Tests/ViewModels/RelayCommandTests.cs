using FluentAssertions;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class RelayCommandTests
{
    [Fact]
    public void Execute_ShouldCallAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithoutPredicate_ShouldReturnTrue()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        canExecute.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithPredicateReturningFalse_ShouldReturnFalse()
    {
        // Arrange
        var command = new RelayCommand(() => { }, () => false);

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        canExecute.Should().BeFalse();
    }

    [Fact]
    public void CanExecute_WithPredicateReturningTrue_ShouldReturnTrue()
    {
        // Arrange
        var command = new RelayCommand(() => { }, () => true);

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        canExecute.Should().BeTrue();
    }

    [Fact]
    public void RaiseCanExecuteChanged_ShouldRaiseCanExecuteChangedEvent()
    {
        // Arrange
        var command = new RelayCommand(() => { });
        var eventRaised = false;
        command.CanExecuteChanged += (s, e) => eventRaised = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithParameter_ShouldCallAction()
    {
        // Arrange
        var receivedValue = 0;
        var command = new RelayCommand<int>(value => receivedValue = value);

        // Act
        command.Execute(42);

        // Assert
        receivedValue.Should().Be(42);
    }

    [Fact]
    public void Execute_WithParameterAndNull_ShouldNotCallAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand<int>(_ => executed = true);

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeFalse();
    }

    [Fact]
    public void CanExecute_WithParameter_ShouldUsePredicate()
    {
        // Arrange
        var command = new RelayCommand<int>(_ => { }, value => value > 0);

        // Act
        var canExecuteTrue = command.CanExecute(1);
        var canExecuteFalse = command.CanExecute(-1);

        // Assert
        canExecuteTrue.Should().BeTrue();
        canExecuteFalse.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithTwoParameters_ShouldCallAction()
    {
        // Arrange
        var receivedValue1 = 0;
        var receivedValue2 = "";
        var command = new RelayCommand<int, string>((val1, val2) =>
        {
            receivedValue1 = val1;
            receivedValue2 = val2;
        });

        // Act
        command.Execute(Tuple.Create(42, "test"));

        // Assert
        receivedValue1.Should().Be(42);
        receivedValue2.Should().Be("test");
    }

    [Fact]
    public void CanExecute_WithTwoParameters_ShouldUsePredicate()
    {
        // Arrange
        var command = new RelayCommand<int, string>((_, _) => { }, (val1, val2) => val1 > 0 && val2.Length > 0);

        // Act
        var canExecuteTrue = command.CanExecute(Tuple.Create(1, "test"));
        var canExecuteFalse = command.CanExecute(Tuple.Create(-1, "test"));

        // Assert
        canExecuteTrue.Should().BeTrue();
        canExecuteFalse.Should().BeFalse();
    }
}
