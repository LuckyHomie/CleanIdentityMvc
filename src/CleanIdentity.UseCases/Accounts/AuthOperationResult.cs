namespace CleanIdentity.UseCases.Accounts;

public sealed record AuthOperationResult(
    bool Succeeded,
    bool IsLockedOut = false,
    bool RequiresPasswordChange = false,
    string? UserId = null,
    string? Email = null,
    string? Error = null)
{
    public static AuthOperationResult Success(string? userId = null, string? email = null) => new(true, UserId: userId, Email: email);
    public static AuthOperationResult Failure(string error) => new(false, Error: error);
}
