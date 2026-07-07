using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanIdentity.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        await dbContext.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, configuration);
        await SeedAllowedIpAddressesAsync(dbContext);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    ThrowIdentityError($"Nie udało się utworzyć roli {role}", result);
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@example.com";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!Admin";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                PasswordChangedAt = DateTimeOffset.UtcNow,
                LockoutEnabled = true,
                ShowActivityAfterLogin = true
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);

            if (!createResult.Succeeded)
            {
                ThrowIdentityError("Nie udało się utworzyć konta administratora", createResult);
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");

            if (!roleResult.Succeeded)
            {
                ThrowIdentityError("Nie udało się przypisać administratora do roli Admin", roleResult);
            }
        }
    }

    private static async Task SeedAllowedIpAddressesAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.AllowedIpAddresses.AnyAsync())
        {
            return;
        }

        dbContext.AllowedIpAddresses.AddRange(
            new AllowedIpAddress
            {
                Value = "127.0.0.1",
                Description = "Localhost IPv4",
                Enabled = true
            },
            new AllowedIpAddress
            {
                Value = "::1",
                Description = "Localhost IPv6",
                Enabled = true
            });

        await dbContext.SaveChangesAsync();
    }

    private static void ThrowIdentityError(string message, IdentityResult result)
    {
        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message}: {errors}");
    }
}