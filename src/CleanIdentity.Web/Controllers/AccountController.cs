using System.Security.Claims;
using CleanIdentity.UseCases.Accounts;
using CleanIdentity.UseCases.Activities;
using CleanIdentity.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CleanIdentity.Web.Controllers;

public sealed class AccountController : Controller
{
    private const string SessionCookieName = "AuthSessionId";
    private readonly IAccountService _accountService;
    private readonly IActivityQueryService _activityQueryService;
    private readonly IUserPreferencesService _preferencesService;

    public AccountController(
        IAccountService accountService,
        IActivityQueryService activityQueryService,
        IUserPreferencesService preferencesService)
    {
        _accountService = accountService;
        _activityQueryService = activityQueryService;
        _preferencesService = preferencesService;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterAsync(new RegisterUserCommand(
            model.Email,
            model.Password,
            model.FirstName,
            model.LastName,
            GetIpAddress(),
            Request.Headers["User-Agent"].ToString()), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nie udało się zarejestrować konta.");
            return View(model);
        }

        TempData["StatusMessage"] = "Konto zostało utworzone. Możesz się zalogować.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var result = await _accountService.LoginAsync(new LoginUserCommand(
            model.Email,
            model.Password,
            model.RememberMe,
            sessionId,
            GetIpAddress(),
            Request.Headers["User-Agent"].ToString()), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nie udało się zalogować.");
            return View(model);
        }

        Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

        if (result.RequiresPasswordChange)
        {
            return RedirectToAction(nameof(ChangePassword), new { expired = true });
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        var showActivity = result.UserId is not null && await _preferencesService.GetShowActivityAfterLoginAsync(result.UserId, cancellationToken);
        return showActivity ? RedirectToAction(nameof(Activity)) : RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Request.Cookies[SessionCookieName] ?? string.Empty;

        if (userId is not null)
        {
            await _accountService.LogoutAsync(userId, sessionId, GetIpAddress(), Request.Headers["User-Agent"].ToString(), cancellationToken);
        }

        Response.Cookies.Delete(SessionCookieName);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tokenResult = await _accountService.CreatePasswordResetTokenAsync(model.Email, cancellationToken);
        if (tokenResult.Succeeded && tokenResult.Token is not null && tokenResult.Email is not null)
        {
            var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(tokenResult.Token));
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { email = tokenResult.Email, token = encodedToken }, Request.Scheme)!;
            await _accountService.SendPasswordResetEmailAsync(tokenResult.Email, callbackUrl, cancellationToken);
        }

        // Celowo zawsze pokazujemy ten sam komunikat, aby nie zdradzać, czy e-mail istnieje w systemie.
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _accountService.ResetPasswordAsync(model.Email, token, model.NewPassword, GetIpAddress(), Request.Headers["User-Agent"].ToString(), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nie udało się zresetować hasła.");
            return View(model);
        }

        TempData["StatusMessage"] = "Hasło zostało zmienione. Możesz się zalogować.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword(bool expired = false)
    {
        return View(new ChangePasswordViewModel { Expired = expired });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _accountService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, GetIpAddress(), Request.Headers["User-Agent"].ToString(), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nie udało się zmienić hasła.");
            return View(model);
        }

        TempData["StatusMessage"] = "Hasło zostało zmienione.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Activity(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var activities = await _activityQueryService.GetForUserAsync(userId, 100, cancellationToken);
        var show = await _preferencesService.GetShowActivityAfterLoginAsync(userId, cancellationToken);
        return View(new ActivityPageViewModel { Activities = activities, ShowAfterLogin = show });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivityTable(bool show, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _preferencesService.SetShowActivityAfterLoginAsync(userId, show, cancellationToken);
        TempData["StatusMessage"] = show ? "Tabela aktywności będzie pokazywana po logowaniu." : "Tabela aktywności nie będzie pokazywana po logowaniu.";
        return RedirectToAction(nameof(Activity));
    }

    public IActionResult AccessDenied() => View();

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
