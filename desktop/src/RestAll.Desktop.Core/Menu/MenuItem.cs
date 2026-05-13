namespace RestAll.Desktop.Core.Menu;

public sealed record MenuItem(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string? PhotoUrl,
    bool IsAvailable,
    int MenuCategoryId,
    string? CategoryName = null
);
