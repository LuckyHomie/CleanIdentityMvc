namespace CleanIdentity.UseCases.Accounts;

public interface IAccountService
{
    Task<AuthOperationResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
    Task<AuthOperationResult> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);
    Task LogoutAsync(string userId, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<PasswordResetTokenResult> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string callbackUrl, CancellationToken cancellationToken = default);
    Task<AuthOperationResult> ResetPasswordAsync(string email, string token, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<AuthOperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
