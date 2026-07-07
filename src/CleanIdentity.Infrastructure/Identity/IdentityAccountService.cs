using CleanIdentity.Infrastructure.Identity;
using CleanIdentity.Infrastructure.Options;
using CleanIdentity.UseCases.Accounts;
using CleanIdentity.UseCases.Activities;
using CleanIdentity.UseCases.Audit;
using CleanIdentity.UseCases.Email;
using CleanIdentity.UseCases.Passwords;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CleanIdentity.Infrastructure.Identity;

public sealed class IdentityAccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPasswordHistoryService _passwordHistory;
    private readonly IAuthAuditService _audit;
    private readonly IActivityLogger _activityLogger;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationSecurityOptions _securityOptions;

    public IdentityAccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPasswordHistoryService passwordHistory,
        IAuthAuditService audit,
        IActivityLogger activityLogger,
        IEmailSender emailSender,
        IOptions<ApplicationSecurityOptions> securityOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordHistory = passwordHistory;
        _audit = audit;
        _activityLogger = activityLogger;
        _emailSender = emailSender;
        _securityOptions = securityOptions.Value;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordChangedAt = DateTimeOffset.UtcNow,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(string.Join(" ", result.Errors.Select(x => x.Description)));
        }
        
        await _userManager.AddToRoleAsync(user, "User");

        await _passwordHistory.RememberCurrentPasswordAsync(user.Id, cancellationToken);
        await _activityLogger.LogAsync(user.Id, "REGISTER", "Użytkownik zarejestrował konto.", command.IpAddress, command.UserAgent, cancellationToken);
        return AuthOperationResult.Success(user.Id, user.Email);
    }

    public async Task<AuthOperationResult> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            await _audit.RecordLoginAsync(null, command.Email, false, false, "Nie znaleziono użytkownika.", command.SessionId, command.IpAddress, command.UserAgent, cancellationToken);
            return AuthOperationResult.Failure("Nieprawidłowy login lub hasło.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, command.Password, command.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            await _audit.RecordLoginAsync(user.Id, user.Email, false, true, "Konto zablokowane po przekroczeniu liczby prób.", command.SessionId, command.IpAddress, command.UserAgent, cancellationToken);
            return new AuthOperationResult(false, IsLockedOut: true, UserId: user.Id, Email: user.Email, Error: "Konto zostało tymczasowo zablokowane.");
        }

        if (!result.Succeeded)
        {
            await _audit.RecordLoginAsync(user.Id, user.Email, false, false, "Nieprawidłowe hasło.", command.SessionId, command.IpAddress, command.UserAgent, cancellationToken);
            return AuthOperationResult.Failure("Nieprawidłowy login lub hasło.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        await _audit.RecordLoginAsync(user.Id, user.Email, true, false, null, command.SessionId, command.IpAddress, command.UserAgent, cancellationToken);
        await _activityLogger.LogAsync(user.Id, "LOGIN", "Użytkownik zalogował się do systemu.", command.IpAddress, command.UserAgent, cancellationToken);

        var requiresPasswordChange = user.PasswordChangedAt is null
            || user.PasswordChangedAt.Value.AddDays(_securityOptions.PasswordMaxAgeDays) < DateTimeOffset.UtcNow;

        return new AuthOperationResult(true, RequiresPasswordChange: requiresPasswordChange, UserId: user.Id, Email: user.Email);
    }

    public async Task LogoutAsync(string userId, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        await _signInManager.SignOutAsync();
        await _audit.RecordLogoutAsync(userId, sessionId, ipAddress, userAgent, cancellationToken);
        await _activityLogger.LogAsync(userId, "LOGOUT", "Użytkownik wylogował się z systemu.", ipAddress, userAgent, cancellationToken);
    }

    public async Task<PasswordResetTokenResult> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Nie ujawniamy, czy e-mail istnieje w bazie.
            return PasswordResetTokenResult.Failure("Jeśli konto istnieje, wiadomość resetująca zostanie wysłana.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return PasswordResetTokenResult.Success(email, token);
    }

    public async Task SendPasswordResetEmailAsync(string email, string callbackUrl, CancellationToken cancellationToken = default)
    {
        var body = $"""
            <p>Otrzymaliśmy prośbę o zresetowanie hasła.</p>
            <p><a href="{callbackUrl}">Kliknij tutaj, aby ustawić nowe hasło</a>.</p>
            <p>Jeśli to nie Ty, zignoruj tę wiadomość.</p>
            """;

        await _emailSender.SendAsync(email, "Reset hasła", body, cancellationToken);
    }

    public async Task<AuthOperationResult> ResetPasswordAsync(string email, string token, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return AuthOperationResult.Failure("Nie udało się zresetować hasła.");
        }

        if (await _passwordHistory.IsReusedPasswordAsync(user.Id, newPassword, _securityOptions.PasswordHistoryLimit, cancellationToken))
        {
            return AuthOperationResult.Failure($"Nie można użyć jednego z ostatnich {_securityOptions.PasswordHistoryLimit} haseł.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(string.Join(" ", result.Errors.Select(x => x.Description)));
        }

        user.PasswordChangedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        await _passwordHistory.RememberCurrentPasswordAsync(user.Id, cancellationToken);
        await _activityLogger.LogAsync(user.Id, "PASSWORD_RESET", "Hasło zostało zresetowane przez link e-mail.", ipAddress, userAgent, cancellationToken);
        return AuthOperationResult.Success(user.Id, user.Email);
    }

    public async Task<AuthOperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthOperationResult.Failure("Nie znaleziono użytkownika.");
        }

        if (user.PasswordChangedAt is not null && user.PasswordChangedAt.Value.AddDays(_securityOptions.PasswordMinAgeDays) > DateTimeOffset.UtcNow)
        {
            return AuthOperationResult.Failure($"Hasło można zmienić dopiero po {_securityOptions.PasswordMinAgeDays} dniu/dniach od ostatniej zmiany.");
        }

        if (await _passwordHistory.IsReusedPasswordAsync(user.Id, newPassword, _securityOptions.PasswordHistoryLimit, cancellationToken))
        {
            return AuthOperationResult.Failure($"Nie można użyć jednego z ostatnich {_securityOptions.PasswordHistoryLimit} haseł.");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(string.Join(" ", result.Errors.Select(x => x.Description)));
        }

        user.PasswordChangedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        await _passwordHistory.RememberCurrentPasswordAsync(user.Id, cancellationToken);
        await _activityLogger.LogAsync(user.Id, "PASSWORD_CHANGE", "Użytkownik zmienił hasło.", ipAddress, userAgent, cancellationToken);
        return AuthOperationResult.Success(user.Id, user.Email);
    }
}
