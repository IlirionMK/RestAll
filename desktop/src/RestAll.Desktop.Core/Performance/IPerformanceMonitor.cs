namespace RestAll.Desktop.Core.Performance;

public interface IPerformanceMonitor
{
    void RecordOperation(string operationName, TimeSpan duration);
    void RecordError(string operationName, Exception exception);
    void RecordMemoryUsage(long memoryUsage);
    Task<PerformanceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}

public class PerformanceMetrics
{
    public Dictionary<string, TimeSpan> OperationDurations { get; set; } = new();
    public Dictionary<string, int> ErrorCounts { get; set; } = new();
    public long CurrentMemoryUsage { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
