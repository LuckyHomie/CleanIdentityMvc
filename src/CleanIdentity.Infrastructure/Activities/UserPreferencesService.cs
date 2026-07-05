using CleanIdentity.Infrastructure.Data;
using CleanIdentity.UseCases.Activities;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Infrastructure.Activities;

public sealed class UserPreferencesService : IUserPreferencesService
{
    private readonly ApplicationDbContext _dbContext;

    public UserPreferencesService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> GetShowActivityAfterLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.ShowActivityAfterLogin)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetShowActivityAfterLoginAsync(string userId, bool show, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstAsync(x => x.Id == userId, cancellationToken);
        user.ShowActivityAfterLogin = show;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
