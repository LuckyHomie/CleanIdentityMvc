namespace CleanIdentity.Infrastructure.Options;

public sealed class ApplicationSecurityOptions
{
    public int PasswordHistoryLimit { get; set; } = 20;
    public int PasswordMaxAgeDays { get; set; } = 90;
    public int PasswordMinAgeDays { get; set; } = 1;
    public string[] AllowedIpAddresses { get; set; } = Array.Empty<string>();
}
