namespace CleanIdentity.Core.Entities;

public sealed class AllowedIpAddress : BaseEntity
{
    public required string Value { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Description { get; set; }
}
