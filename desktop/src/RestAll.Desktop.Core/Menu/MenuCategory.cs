namespace RestAll.Desktop.Core.Menu;

public sealed record MenuCategory(
    int Id,
    string Name,
    int SortOrder,
    List<MenuItem> Items
);
