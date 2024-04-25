using System.Diagnostics;
using DeepfakeWeb.Data;
using DeepfakeWeb.Models;
using DeepfakeWeb.Models.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeepfakeWeb.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _dbContext;

    public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
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
            return RedirectToAction("Index", "Upload");
        }

        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }

    [HttpPost]
    public async Task<IActionResult> OnPostFeedBack(Feedback? feedback)
    {
        
        if (feedback == null)
        {
            TempData["error"] = "Feedback not found.";
            return RedirectToAction("History", "Upload");
        }

        if (string.IsNullOrEmpty(feedback.Title) || string.IsNullOrEmpty(feedback.Description))
        {
            TempData["error"] = "Title and Description are required.";
            return RedirectToAction("GetFeedBack", new { id = feedback.ImageId });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["error"] = "You must be logged in to submit feedback.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        feedback.AppUserId = user.Id;
        feedback.AppUserEmail = user.Email ?? "name@email.com";
        _dbContext.Feedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync();
        TempData["success"] = "Feedback submitted successfully.";
        return RedirectToAction("History", "Upload");
    }


    public async Task<IActionResult> GetFeedBack(int? id)
    {
        if (id == null)
        {
            TempData["error"] = "Image not found.";
            return RedirectToAction("History", "Upload");
        }

        var appUser = await _userManager.GetUserAsync(User);

        if (appUser == null)
        {
            TempData["error"] = "You must be logged in to view feedback.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var image = await _dbContext.ImageData.FirstOrDefaultAsync(i => i.Id == id);
        if (image == null)
        {
            TempData["error"] = "Image not found. Retry again.";
            return RedirectToAction("History", "Upload");
        }


        var feedback = new Feedback
        {
            Title = "",
            Description = "",
            AppUserId = appUser.Id,
            ImageId = id.Value,
            ImageData = image,
            AppUserEmail = appUser.Email ?? "name@email.com"
        };

        return View(feedback);
    }

    public async Task<IActionResult> GetAllFeedback()
    {
        var appUser = await _userManager.GetUserAsync(User);

        if (appUser == null)
        {
            TempData["error"] = "You must be logged in to view feedback.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        
        if (!appUser.IsAdmin())
        {
            TempData["error"] = "You must be an admin to view feedback all feedbacks.";
            return RedirectToAction("Index", "Home");
        }
        var feedbacks = await _dbContext.Feedbacks.ToListAsync();
        var imagesIds = feedbacks.Select(f => f.ImageId).ToList();
        var images = await _dbContext.ImageData.Where(i => imagesIds.Contains(i.Id)).ToListAsync();
        foreach (var feedback in feedbacks)
        {
            feedback.ImageData = images.First(i => i.Id == feedback.ImageId);
        }

        return View(feedbacks);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}