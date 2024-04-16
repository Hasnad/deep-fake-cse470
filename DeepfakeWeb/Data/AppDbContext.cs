using DeepfakeWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeepfakeWeb.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ImageData> ImageData { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ImageData>()
            .Property(p => p.GenerationSourcesLikeness)
            .HasConversion(p => JsonConvert.SerializeObject(p),
                p => JsonConvert.DeserializeObject<List<GenerationSource>>(p)!);
    }
}