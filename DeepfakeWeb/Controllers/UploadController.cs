using DeepfakeWeb.Data;
using DeepfakeWeb.Models;
using DeepfakeWeb.Models.JsonModel;
using DeepfakeWeb.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeepfakeWeb.Controllers;

public class UploadController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public UploadController(UserManager<AppUser> userManager, AppDbContext dbContext,
        HttpClient httpClient)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(UploadIndex model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            TempData["error"] = "You must be logged in to upload an image.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
        
        
        if (model.FileToUpload != null && model.FileToUpload.Length > 0)
        {
            if (!user.HasSubscription())
            {
                if (model.FileToUpload.Length > 1 * 1024 * 1024)
                {
                    TempData["error"] = "The file size must be less than 1MB. Otherwise Subscribe to our service.";
                    return View();
                } 
            }
            
            var extension = Path.GetExtension(model.FileToUpload.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("FileToUpload", "Only .jpg, .jpeg, and .png files are allowed.");
                return View(model);
            }

            var imageToBase64 = (await model.FileToUpload.GetBytesAsync()).GetBase64();

            var responseMessage =
                await _httpClient.PostAsJsonAsync("http://127.0.0.1:5000/process_image",
                    JsonConvert.SerializeObject(new { image = imageToBase64 }));

            if (!responseMessage.IsSuccessStatusCode)
            {
                TempData["error"] = "An error occurred while processing the image.";
                return View();
            }

            var response = await responseMessage.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<ImageAnalysisModel>(response);
            if (responseContent == null)
            {
                TempData["error"] = "An error occurred while processing the image.";
                return View();
            }

            if (!responseContent.Success)
            {
                TempData["error"] = responseContent.Message;
                return View();
            }

            

            var image = new ImageData
            {
                FileName = Guid.NewGuid() + extension,
                MaskImageBase64 = responseContent.FaceWithMask,
                OriginalImageBase64 = imageToBase64,
                Confidence = responseContent.GetConfidence(),
                IsReal = responseContent.IsReal(),
                AppUserId = user.Id,
                GenerationSourcesLikeness = responseContent.GetGenerationSources()
            };


            user.ImageDataList.Add(image);
            _dbContext.ImageData.Add(image);
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "Image uploaded successfully!";
            return View("Analysis", image);
        }
        else
        {
            TempData["error"] = "Please select a file to upload.";
        }

        return View();
    }

    public async Task<IActionResult> History()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["error"] = "You must be logged in to view your history.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        if (!user.HasSubscription())
        {
            TempData["error"] = "You must have an active subscription to view the analysis.";
            return RedirectToAction("Pricing","Home");
        }
        
        var images = await _dbContext.ImageData.Where(i => i.AppUserId == user.Id).ToListAsync();


        if (images.Count == 0)
        {
            TempData["error"] = "You have not uploaded any images yet.";
            return RedirectToAction("Index");
        }

        user.ImageDataList = images;

        return View(user.ImageDataList);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var imageData = await _dbContext.ImageData.FindAsync(id);
        if (imageData == null)
        {
            TempData["error"] = "Image not found.";
            return RedirectToAction("History");
        }

        _dbContext.ImageData.Remove(imageData);
        await _dbContext.SaveChangesAsync();
        TempData["success"] = "Image deleted successfully!";
        return RedirectToAction("History");
    }

    public async Task<IActionResult> ViewAnalysis(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["error"] = "You must be logged in to view your history.";
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        if (!user.HasSubscription())
        {
            TempData["error"] = "You must have an active subscription to view the analysis.";
            return RedirectToAction("Pricing","Home");
        }

        var imageData = await _dbContext.ImageData.FindAsync(id);
        if (imageData == null)
        {
            TempData["error"] = "Image not found.";
            return RedirectToAction("History");
        }

        return View(imageData);
    }
}