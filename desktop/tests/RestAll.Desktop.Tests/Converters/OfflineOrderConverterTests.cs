using System.Globalization;
using FluentAssertions;
using RestAll.Desktop.App.Converters;
using RestAll.Desktop.Core.Orders;
using Xunit;

namespace RestAll.Desktop.Tests.Converters;

public class OfflineOrderConverterTests
{
    private readonly OfflineOrderConverter _converter = new();

    [Fact]
    public void Convert_WithOfflineOrder_ShouldReturnVisibleForVisibility()
    {
        // Arrange
        var offlineOrder = new Order(-123456789, 1, 0, 0m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(offlineOrder, typeof(System.Windows.Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(System.Windows.Visibility.Visible);
    }

    [Fact]
    public void Convert_WithOnlineOrder_ShouldReturnCollapsedForVisibility()
    {
        // Arrange
        var onlineOrder = new Order(123, 1, 1, 29.99m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(onlineOrder, typeof(System.Windows.Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(System.Windows.Visibility.Collapsed);
    }

    [Fact]
    public void Convert_WithOfflineOrder_ShouldReturnTrueForBoolean()
    {
        // Arrange
        var offlineOrder = new Order(-123456789, 1, 0, 0m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(offlineOrder, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WithOnlineOrder_ShouldReturnFalseForBoolean()
    {
        // Arrange
        var onlineOrder = new Order(123, 1, 1, 29.99m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(onlineOrder, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WithOfflineOrder_ShouldReturnOrangeBrush()
    {
        // Arrange
        var offlineOrder = new Order(-123456789, 1, 0, 0m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(offlineOrder, typeof(System.Windows.Media.Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(System.Windows.Media.Brushes.Orange);
    }

    [Fact]
    public void Convert_WithOnlineOrder_ShouldReturnBlackBrush()
    {
        // Arrange
        var onlineOrder = new Order(123, 1, 1, 29.99m, OrderStatus.Pending, new List<OrderItem>());

        // Act
        var result = _converter.Convert(onlineOrder, typeof(System.Windows.Media.Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(System.Windows.Media.Brushes.Black);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<System.NotImplementedException>(
            () => _converter.ConvertBack(null!, typeof(bool), null!, CultureInfo.InvariantCulture)
        );
    }
}
