namespace RestAll.Desktop.Core.Auth;

public sealed record UserSession(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Role
);
