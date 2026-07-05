using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Infrastructure.Data;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<LoginAuditLog> LoginAuditLogs => Set<LoginAuditLog>();
    public DbSet<AllowedIpAddress> AllowedIpAddresses => Set<AllowedIpAddress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserActivity>(entity =>
        {
            entity.ToTable("UserActivities");
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
        });

        builder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("PasswordHistories");
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
        });

        builder.Entity<LoginAuditLog>(entity =>
        {
            entity.ToTable("LoginAuditLogs");
            entity.Property(x => x.UserId).HasMaxLength(450);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.Property(x => x.SessionId).HasMaxLength(128);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.HasIndex(x => x.SessionId);
        });

        builder.Entity<AllowedIpAddress>(entity =>
        {
            entity.ToTable("AllowedIpAddresses");
            entity.Property(x => x.Value).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256);
            entity.HasIndex(x => x.Value).IsUnique();
        });
    }
}
