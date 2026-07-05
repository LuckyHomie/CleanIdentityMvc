namespace CleanIdentity.UseCases.Activities;

public interface IActivityLogger
{
    Task LogAsync(string userId, string action, string? details, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
