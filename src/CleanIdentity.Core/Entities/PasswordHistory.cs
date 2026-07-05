namespace CleanIdentity.Core.Entities;

public sealed class PasswordHistory : BaseEntity
{
    public required string UserId { get; set; }
    public required string PasswordHash { get; set; }
}
