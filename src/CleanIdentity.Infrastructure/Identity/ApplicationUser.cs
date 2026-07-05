using Microsoft.AspNetCore.Identity;

namespace CleanIdentity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
    public bool ShowActivityAfterLogin { get; set; } = true;
}
