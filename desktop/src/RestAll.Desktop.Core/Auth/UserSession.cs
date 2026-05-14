namespace RestAll.Desktop.Core.Auth;

public sealed record UserSession(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Role
)
{
    /// <summary>
    /// Create a session from profile data without tokens (cookie-based auth).
    /// </summary>
    public static UserSession FromProfile(string fullName, string role)
    {
        return new UserSession(string.Empty, string.Empty, fullName, role);
    }
}

