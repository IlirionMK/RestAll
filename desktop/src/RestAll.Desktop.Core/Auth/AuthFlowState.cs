namespace RestAll.Desktop.Core.Auth;

public enum AuthFlowState
{
    Anonymous = 0,
    RequiresTwoFactor = 1,
    Authenticated = 2
}
