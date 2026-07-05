namespace CleanIdentity.UseCases.Activities;

public interface IUserPreferencesService
{
    Task<bool> GetShowActivityAfterLoginAsync(string userId, CancellationToken cancellationToken = default);
    Task SetShowActivityAfterLoginAsync(string userId, bool show, CancellationToken cancellationToken = default);
}
