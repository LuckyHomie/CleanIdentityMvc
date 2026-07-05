namespace CleanIdentity.UseCases.Accounts;

public sealed record PasswordResetTokenResult(bool Succeeded, string? Email = null, string? Token = null, string? Error = null)
{
    public static PasswordResetTokenResult Success(string email, string token) => new(true, email, token);
    public static PasswordResetTokenResult Failure(string error) => new(false, Error: error);
}
