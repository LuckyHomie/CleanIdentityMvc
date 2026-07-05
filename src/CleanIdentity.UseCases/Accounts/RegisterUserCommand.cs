namespace CleanIdentity.UseCases.Accounts;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? IpAddress,
    string? UserAgent);
