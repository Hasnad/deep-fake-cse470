using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeepfakeWeb.Data;

namespace DeepfakeWeb.Models.Home;

public class Feedback
{
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required] public int ImageId { get; set; }
    [Required] public required string AppUserId { get; set; }
    [Required, MaxLength(255)] public required string Title { get; set; }
    [Required, MaxLength(1000),Display(Name = "Feedback")] public required string Description { get; set; }
    
    [NotMapped]
    public ImageData ImageData { get; set; }
    
    [NotMapped, Display(Name = "Email")]
    public string AppUserEmail { get; set; }
    
}