using Microsoft.AspNetCore.Mvc;

namespace CleanIdentity.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
}
