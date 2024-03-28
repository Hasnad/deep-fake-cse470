using DeepfakeWeb.Data;
using Microsoft.AspNetCore.Identity;

namespace DeepfakeWeb.Models;

public class ApplicationUser : IdentityUser
{
    public List<ImageData> ImageDataList { get; set; } = new();
}