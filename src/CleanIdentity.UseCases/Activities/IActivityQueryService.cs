namespace CleanIdentity.UseCases.Activities;

public interface IActivityQueryService
{
    Task<IReadOnlyList<UserActivityDto>> GetForUserAsync(string userId, int take = 50, CancellationToken cancellationToken = default);
}
