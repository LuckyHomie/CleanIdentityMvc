namespace CleanIdentity.UseCases.Audit;

public interface IAuthAuditService
{
    Task RecordLoginAsync(string? userId, string? email, bool success, bool lockedOut, string? failureReason, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task RecordLogoutAsync(string userId, string sessionId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
