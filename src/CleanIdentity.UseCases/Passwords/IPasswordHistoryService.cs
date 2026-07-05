namespace CleanIdentity.UseCases.Passwords;

public interface IPasswordHistoryService
{
    Task<bool> IsReusedPasswordAsync(string userId, string newPassword, int historyLimit, CancellationToken cancellationToken = default);
    Task RememberCurrentPasswordAsync(string userId, CancellationToken cancellationToken = default);
}
