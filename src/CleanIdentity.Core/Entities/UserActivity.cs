namespace CleanIdentity.Core.Entities;

public sealed class UserActivity : BaseEntity
{
    public required string UserId { get; set; }
    public required string Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
