namespace RestAll.Desktop.Core.Auth;

public sealed record UserProfile(
    int Id,
    string Name,
    string Email,
    string Role,
    int? RestaurantId = null
);
