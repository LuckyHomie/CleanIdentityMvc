using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Data;
using CleanIdentity.UseCases.Activities;

namespace CleanIdentity.Infrastructure.Activities;

public sealed class ActivityLogger : IActivityLogger
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityLogger(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(string userId, string action, string? details, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        _dbContext.UserActivities.Add(new UserActivity
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
