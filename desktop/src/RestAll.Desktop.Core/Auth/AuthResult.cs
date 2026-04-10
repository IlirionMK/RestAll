namespace RestAll.Desktop.Core.Auth;

public sealed record AuthResult(
    AuthFlowState State,
    string Message,
    UserSession? Session = null,
    string? TwoFactorTicket = null
);
