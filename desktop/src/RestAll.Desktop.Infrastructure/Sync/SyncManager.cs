using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Infrastructure.Offline;

namespace RestAll.Desktop.Infrastructure.Sync;

public class SyncManager : ISyncManager
{
    private readonly SqliteOfflineStorage _offlineStorage;
    private readonly IOrderGateway _orderGateway;
    private readonly ILogger<SyncManager> _logger;
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    public SyncManager(SqliteOfflineStorage offlineStorage, IOrderGateway orderGateway, ILogger<SyncManager> logger)
    {
        _offlineStorage = offlineStorage;
        _orderGateway = orderGateway;
        _logger = logger;
    }

    public void Start()
    {
        if (_runTask is not null && !_runTask.IsCompleted) return;
        _cts = new CancellationTokenSource();
        _runTask = Task.Run(() => RunAsync(_cts.Token));
        _logger.LogInformation("SyncManager started");
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
        }
        catch { }
    }

    public async Task<Order> EnqueueCreateOrderAsync(int tableId, CancellationToken cancellationToken = default)
    {
        var opId = Guid.NewGuid().ToString();
        var payload = JsonSerializer.Serialize(new { table_id = tableId });
        await _offlineStorage.EnqueueOperationAsync(opId, "CreateOrder", payload, cancellationToken);

        // create temporary local order to show in UI
        var tempId = GenerateTemporaryId();
        var tempOrder = new Order(tempId, tableId, 0, 0m, OrderStatus.Pending, new List<OrderItem>());
        return tempOrder;
    }

    private static int GenerateTemporaryId()
    {
        var guid = Guid.NewGuid();
        var hash = Math.Abs(guid.GetHashCode());
        return -1 * (hash == 0 ? 1 : hash);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var ops = await _offlineStorage.GetPendingOperationsAsync(cancellationToken);
                foreach (var (id, type, payload, attempts) in ops)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (type == "CreateOrder")
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(payload);
                            var root = doc.RootElement;
                            if (!root.TryGetProperty("table_id", out var tableEl) || !tableEl.TryGetInt32(out var tableId))
                            {
                                _logger.LogWarning("Invalid payload for CreateOrder op {Id}", id);
                                await _offlineStorage.RemoveOperationAsync(id, cancellationToken);
                                continue;
                            }

                            var created = await _orderGateway.CreateOrderAsync(tableId, cancellationToken);
                            if (created is not null)
                            {
                                // success
                                await _offlineStorage.RemoveOperationAsync(id, cancellationToken);
                                // persist server order locally
                                await _offlineStorage.SaveOrderAsync(created, cancellationToken);
                                _logger.LogInformation("Flushed CreateOrder op {Id} -> order {OrderId}", id, created.Id);
                            }
                            else
                            {
                                // increment attempts
                                await _offlineStorage.IncrementOperationAttemptsAsync(id, cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error flushing operation {Id}", id);
                            try
                            {
                                await _offlineStorage.IncrementOperationAttemptsAsync(id, cancellationToken);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Unknown operation type in queue: {Type}", type);
                        await _offlineStorage.RemoveOperationAsync(id, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncManager loop error");
            }

            // Sleep before next attempt
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
            catch (TaskCanceledException) { }
        }
    }
}

