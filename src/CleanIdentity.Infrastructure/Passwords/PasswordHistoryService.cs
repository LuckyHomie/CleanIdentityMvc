using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Data;
using CleanIdentity.Infrastructure.Identity;
using CleanIdentity.UseCases.Passwords;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Infrastructure.Passwords;

public sealed class PasswordHistoryService : IPasswordHistoryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public PasswordHistoryService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> IsReusedPasswordAsync(string userId, string newPassword, int historyLimit, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var hashes = await _dbContext.PasswordHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(historyLimit)
            .Select(x => x.PasswordHash)
            .ToListAsync(cancellationToken);

        return hashes.Any(hash => _passwordHasher.VerifyHashedPassword(user, hash, newPassword) != PasswordVerificationResult.Failed);
    }

    public async Task RememberCurrentPasswordAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user?.PasswordHash is null)
        {
            return;
        }

        _dbContext.PasswordHistories.Add(new PasswordHistory
        {
            UserId = userId,
            PasswordHash = user.PasswordHash
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
