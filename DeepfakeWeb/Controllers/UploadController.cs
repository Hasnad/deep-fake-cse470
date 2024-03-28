using DeepfakeWeb.Data;
using DeepfakeWeb.Models;
using DeepfakeWeb.Models.JsonModel;
using DeepfakeWeb.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeepfakeWeb.Controllers;

public class UploadController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public UploadController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext,
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
        if (model.FileToUpload != null && model.FileToUpload.Length > 0)
        {
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
                Id = Guid.NewGuid(),
                FileName = Guid.NewGuid() + extension,
                ImageFileBase64 = imageToBase64
            };

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["error"] = "You must be logged in to upload an image.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            user.ImageDataList.Add(image);
            ;
            _dbContext.ImageData.Add(image);
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "Image uploaded successfully!";
            return View("Analysis", responseContent);
        }
        else
        {
            TempData["error"] = "Please select a file to upload.";
        }

        return View();
    }
}