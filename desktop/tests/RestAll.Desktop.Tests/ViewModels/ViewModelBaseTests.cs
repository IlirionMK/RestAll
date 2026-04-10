using FluentAssertions;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.ViewModels;

public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = "";

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        private int _numberProperty = 0;

        public int NumberProperty
        {
            get => _numberProperty;
            set => SetProperty(ref _numberProperty, value);
        }
    }

    [Fact]
    public void SetProperty_WhenValueChanges_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.TestProperty = "New Value";

        // Assert
        propertyChangedRaised.Should().BeTrue();
        viewModel.TestProperty.Should().Be("New Value");
    }

    [Fact]
    public void SetProperty_WhenValueDoesNotChange_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        viewModel.TestProperty = "Initial Value";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.TestProperty = "Initial Value";

        // Assert
        propertyChangedRaised.Should().BeFalse();
    }

    [Fact]
    public void SetProperty_WithSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.TestProperty = "";

        // Assert
        propertyChangedRaised.Should().BeFalse();
    }

    [Fact]
    public void SetProperty_WithIntValue_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) => propertyChangedRaised = true;

        // Act
        viewModel.NumberProperty = 42;

        // Assert
        propertyChangedRaised.Should().BeTrue();
        viewModel.NumberProperty.Should().Be(42);
    }

    [Fact]
    public void PropertyChanged_ShouldContainPropertyName()
    {
        // Arrange
        var viewModel = new TestViewModel();
        string? propertyName = null;
        viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        // Act
        viewModel.TestProperty = "New Value";

        // Assert
        propertyName.Should().Be(nameof(TestViewModel.TestProperty));
    }
}
