using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Data;
using CleanIdentity.UseCases.Audit;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Infrastructure.Audit;

public sealed class AuthAuditService : IAuthAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthAuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordLoginAsync(string? userId, string? email, bool success, bool lockedOut, string? failureReason, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        _dbContext.LoginAuditLogs.Add(new LoginAuditLog
        {
            UserId = userId,
            Email = email,
            Success = success,
            LockedOut = lockedOut,
            FailureReason = failureReason,
            SessionId = sessionId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordLogoutAsync(string userId, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var login = await _dbContext.LoginAuditLogs
            .Where(x => x.UserId == userId && x.SessionId == sessionId && x.Success && x.LogoutAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (login is not null)
        {
            login.LogoutAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
