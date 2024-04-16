using System.Diagnostics;
using System.Text.Json;
using DeepfakeWeb.Data;
using Microsoft.AspNetCore.Mvc;
using DeepfakeWeb.Models;
using DeepfakeWeb.Models.JsonModel;
using DeepfakeWeb.Utils;
using Microsoft.AspNetCore.Identity;

namespace DeepfakeWeb.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Pricing()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["error"] = "You must be logged in to view your subscription.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        if (!user.HasSubscription())
        {
            user.SubscriptionActive = true;
            user.SubscriptionExpirationDate = DateTime.UtcNow.AddDays(30);
            await _userManager.UpdateAsync(user);
            TempData["success"] = "Your subscription has been activated.";
        }
        else
        {
            TempData["success"] = "Your subscription is already active.";
        }

        return Ok();
    }

    public IActionResult TryForFree()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index","Upload");
        }
        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}