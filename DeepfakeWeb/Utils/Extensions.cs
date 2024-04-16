using System.Security.Claims;
using DeepfakeWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeepfakeWeb.Utils;

public static class Extensions
{
    public static async Task<byte[]> GetBytesAsync(this IFormFile formFile)
    {
        await using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static string GetBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static byte[] GetBytesFromBase64(this string base64)
    {
        return Convert.FromBase64String(base64);
    }

    public static AppUser? GetFullUserAsync(this UserManager<AppUser> userManager, ClaimsPrincipal userPrincipal)
    {
        var user = userManager.GetUserId(userPrincipal);
        if (user == null)
        {
            return null;
        }

        var myUser = userManager.Users
            .Include(u => u.ImageDataList)
            .First(u => u.Id == user);
        return myUser;
    }
}