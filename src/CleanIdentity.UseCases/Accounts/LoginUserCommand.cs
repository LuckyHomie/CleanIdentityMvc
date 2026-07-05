namespace CleanIdentity.UseCases.Accounts;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    bool RememberMe,
    string SessionId,
    string? IpAddress,
    string? UserAgent);
