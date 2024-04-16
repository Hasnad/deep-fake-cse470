using DeepfakeWeb.Data;
using Microsoft.AspNetCore.Identity;

namespace DeepfakeWeb.Models;

public class AppUser : IdentityUser
{
    public List<ImageData> ImageDataList { get; set; } = new();
    
    public bool SubscriptionActive { get; set; }
    
    public DateTime SubscriptionExpirationDate { get; set; }
    
    
    public bool HasSubscription()
    {
        return SubscriptionActive && SubscriptionExpirationDate > DateTime.UtcNow;
    }
}