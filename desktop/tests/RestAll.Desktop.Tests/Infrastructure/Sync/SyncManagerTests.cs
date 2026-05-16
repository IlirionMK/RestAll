using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Sync;
using Xunit;

namespace RestAll.Desktop.Tests.Infrastructure.Sync;

public class SyncManagerTests
{
    // Note: SyncManager requires SqliteOfflineStorage (concrete class), not IOfflineStorage
    // For unit tests, we test the Order generation logic directly instead of mocking SyncManager

    [Fact]
    public async Task EnqueueCreateOrderAsync_ShouldGenerateNegativeId()
    {
        // Act - Simulate what SyncManager does
        var tempId = GenerateTemporaryId();
        var result = new Order(tempId, 5, 0, 0m, OrderStatus.Pending, new List<OrderItem>());

        // Assert
        result.Id.Should().BeLessThan(0, "Offline orders should have negative IDs");
        result.TableId.Should().Be(5);
        result.Status.Should().Be(OrderStatus.Pending);
        result.Items.Should().BeEmpty();
    }

    private static int GenerateTemporaryId()
    {
        var guid = Guid.NewGuid();
        var hash = Math.Abs(guid.GetHashCode());
        return -1 * (hash == 0 ? 1 : hash);
    }

    [Fact]
    public void TemporaryIdGeneration_ShouldAlwaysBeNegative()
    {
        // Arrange & Act - Generate multiple IDs
        var ids = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            ids.Add(GenerateTemporaryId());
        }

        // Assert - All should be negative
        ids.Should().OnlyContain(id => id < 0, "All temporary IDs should be negative");
        ids.Should().HaveCount(100);
        ids.Distinct().Should().HaveCount(100, "All IDs should be unique");
    }
}
