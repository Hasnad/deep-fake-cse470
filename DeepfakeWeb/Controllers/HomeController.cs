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
    
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Pricing()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    
}