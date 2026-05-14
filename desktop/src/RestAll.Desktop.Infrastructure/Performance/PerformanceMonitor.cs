using Microsoft.Extensions.Logging;
using RestAll.Desktop.Core.Performance;

namespace RestAll.Desktop.Infrastructure.Performance;

public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly Dictionary<string, List<TimeSpan>> _operationHistory = new();
    private readonly Dictionary<string, int> _errorCounts = new();
    private readonly object _lock = new object();

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public void RecordOperation(string operationName, TimeSpan duration)
    {
        lock (_lock)
        {
            if (!_operationHistory.ContainsKey(operationName))
            {
                _operationHistory[operationName] = new List<TimeSpan>();
            }

            _operationHistory[operationName].Add(duration);
            
            // Keep only last 100 operations per operation type
            if (_operationHistory[operationName].Count > 100)
            {
                _operationHistory[operationName].RemoveRange(0, 50);
            }

            _logger.LogDebug("Operation {OperationName} completed in {Duration}ms", 
                operationName, duration.TotalMilliseconds);
        }
    }

    public void RecordError(string operationName, Exception exception)
    {
        lock (_lock)
        {
            if (!_errorCounts.ContainsKey(operationName))
            {
                _errorCounts[operationName] = 0;
            }

            _errorCounts[operationName]++;
            
            _logger.LogError(exception, "Error in operation {OperationName} (count: {ErrorCount})", 
                operationName, _errorCounts[operationName]);
        }
    }

    public void RecordMemoryUsage(long memoryUsage)
    {
        _logger.LogDebug("Memory usage: {MemoryUsage}MB", memoryUsage / 1024 / 1024);
    }

    public async Task<PerformanceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var metrics = new PerformanceMetrics
                {
                    LastUpdated = DateTime.UtcNow,
                    CurrentMemoryUsage = GC.GetTotalMemory(false)
                };

                // Calculate average durations for each operation
                foreach (var kvp in _operationHistory)
                {
                    if (kvp.Value.Count > 0)
                    {
                        var average = TimeSpan.FromTicks((long)kvp.Value.Average(ts => ts.Ticks));
                        metrics.OperationDurations[kvp.Key] = average;
                    }
                }

                // Copy error counts
                foreach (var kvp in _errorCounts)
                {
                    metrics.ErrorCounts[kvp.Key] = kvp.Value;
                }

                _logger.LogDebug("Performance metrics generated: {OperationCount} operations, {ErrorCount} errors", 
                    metrics.OperationDurations.Count, metrics.ErrorCounts.Count);

                return metrics;
            }
        }, cancellationToken);
    }
}
