namespace RestAll.Desktop.Core.Tables;

public sealed record Table(
    int Id,
    string Number,
    int Capacity,
    TableStatus Status
);
