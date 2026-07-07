namespace CleanIdentity.Web.Models.Admin;

public sealed class AdminDashboardViewModel
{
    public List<AdminUserListItemViewModel> Users { get; set; } = [];
    public List<AdminActivityListItemViewModel> RecentActivities { get; set; } = [];
    public List<AdminAllowedIpListItemViewModel> AllowedIpAddresses { get; set; } = [];
}

public sealed class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public IList<string> Roles { get; set; } = [];

    public bool IsLockedOut =>
        LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;
}

public sealed class AdminActivityListItemViewModel
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? Email { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

public sealed class AdminAllowedIpListItemViewModel
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Enabled { get; set; }
}