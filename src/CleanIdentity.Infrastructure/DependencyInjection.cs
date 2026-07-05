using CleanIdentity.Infrastructure.Activities;
using CleanIdentity.Infrastructure.Audit;
using CleanIdentity.Infrastructure.Data;
using CleanIdentity.Infrastructure.Email;
using CleanIdentity.Infrastructure.Identity;
using CleanIdentity.Infrastructure.Options;
using CleanIdentity.Infrastructure.Passwords;
using CleanIdentity.UseCases.Accounts;
using CleanIdentity.UseCases.Activities;
using CleanIdentity.UseCases.Audit;
using CleanIdentity.UseCases.Email;
using CleanIdentity.UseCases.Passwords;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanIdentity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApplicationSecurityOptions>(configuration.GetSection("Security"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 3;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        });

        services.AddScoped<IAccountService, IdentityAccountService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<IActivityQueryService, ActivityQueryService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<IAuthAuditService, AuthAuditService>();
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
