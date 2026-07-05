using CleanIdentity.Infrastructure.Data;
using CleanIdentity.UseCases.Activities;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Infrastructure.Activities;

public sealed class ActivityQueryService : IActivityQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserActivityDto>> GetForUserAsync(string userId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new UserActivityDto(x.Id, x.CreatedAt, x.Action, x.Details, x.IpAddress, x.UserAgent))
            .ToListAsync(cancellationToken);
    }
}
