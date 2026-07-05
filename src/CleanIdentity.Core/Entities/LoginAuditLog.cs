namespace CleanIdentity.Core.Entities;

public sealed class LoginAuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public bool Success { get; set; }
    public bool LockedOut { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public DateTimeOffset? LogoutAt { get; set; }
}
