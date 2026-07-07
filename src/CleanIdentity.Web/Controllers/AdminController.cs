using CleanIdentity.Core.Entities;
using CleanIdentity.Infrastructure.Data;
using CleanIdentity.Infrastructure.Identity;
using CleanIdentity.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanIdentity.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .OrderBy(x => x.Email)
            .ToListAsync(cancellationToken);

        var userItems = new List<AdminUserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userItems.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles
            });
        }

        var recentActivities = await _dbContext.UserActivities
            .AsNoTracking()
            .Join(
                _dbContext.Users,
                activity => activity.UserId,
                user => user.Id,
                (activity, user) => new AdminActivityListItemViewModel
                {
                    CreatedAt = activity.CreatedAt,
                    Email = user.Email,
                    Action = activity.Action,
                    Details = activity.Details,
                    IpAddress = activity.IpAddress
                })
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .ToListAsync(cancellationToken);

        var allowedIps = await _dbContext.AllowedIpAddresses
            .AsNoTracking()
            .OrderBy(x => x.Value)
            .Select(x => new AdminAllowedIpListItemViewModel
            {
                Id = x.Id,
                Value = x.Value,
                Description = x.Description,
                Enabled = x.Enabled
            })
            .ToListAsync(cancellationToken);

        return View(new AdminDashboardViewModel
        {
            Users = userItems,
            RecentActivities = recentActivities,
            AllowedIpAddresses = allowedIps
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            TempData["StatusMessage"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));

        TempData["StatusMessage"] = $"Zablokowano użytkownika {user.Email} na 15 minut.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            TempData["StatusMessage"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        TempData["StatusMessage"] = $"Odblokowano użytkownika {user.Email}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAdminRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            TempData["StatusMessage"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        TempData["StatusMessage"] = $"Dodano rolę Admin użytkownikowi {user.Email}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdminRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            TempData["StatusMessage"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId)
        {
            TempData["StatusMessage"] = "Nie możesz odebrać roli Admin samemu sobie.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        TempData["StatusMessage"] = $"Odebrano rolę Admin użytkownikowi {user.Email} i przypisano rolę User.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAllowedIp(string value, string? description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            TempData["StatusMessage"] = "Adres IP nie może być pusty.";
            return RedirectToAction(nameof(Index));
        }

        var exists = await _dbContext.AllowedIpAddresses
            .AnyAsync(x => x.Value == value, cancellationToken);

        if (!exists)
        {
            _dbContext.AllowedIpAddresses.Add(new AllowedIpAddress
            {
                Value = value.Trim(),
                Description = description,
                Enabled = true
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        TempData["StatusMessage"] = $"Dodano adres IP {value}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAllowedIp(int id, CancellationToken cancellationToken)
    {
        var ip = await _dbContext.AllowedIpAddresses
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ip is null)
        {
            TempData["StatusMessage"] = "Nie znaleziono adresu IP.";
            return RedirectToAction(nameof(Index));
        }

        ip.Enabled = !ip.Enabled;

        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = ip.Enabled
            ? $"Włączono adres IP {ip.Value}."
            : $"Wyłączono adres IP {ip.Value}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAllowedIp(int id, CancellationToken cancellationToken)
    {
        var ip = await _dbContext.AllowedIpAddresses
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ip is not null)
        {
            _dbContext.AllowedIpAddresses.Remove(ip);
            await _dbContext.SaveChangesAsync(cancellationToken);

            TempData["StatusMessage"] = $"Usunięto adres IP {ip.Value}.";
        }

        return RedirectToAction(nameof(Index));
    }
}