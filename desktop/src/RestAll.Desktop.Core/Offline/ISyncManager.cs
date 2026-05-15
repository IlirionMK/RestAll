using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.Core.Offline;

public interface ISyncManager
{
    void Start();
    void Stop();

    // Enqueue create order operation. Returns a locally-created Order (temporary id) to show in UI.
    Task<Order> EnqueueCreateOrderAsync(int tableId, CancellationToken cancellationToken = default);
}

