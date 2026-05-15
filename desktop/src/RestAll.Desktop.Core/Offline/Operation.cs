using System.Text.Json.Serialization;

namespace RestAll.Desktop.Core.Offline;

public sealed class Operation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public int Attempts { get; init; } = 0;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

