using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeepfakeWeb.Data;

public class ImageData
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(255)] public required string FileName { get; set; }
    [MaxLength(255)] public required string MaskImageBase64 { get; set; }
    [MaxLength(255)] public required string OriginalImageBase64 { get; set; }

    public bool IsReal { get; set; }

    public double Confidence { get; set; }

    public string AppUserId { get; set; }

    public List<GenerationSource> GenerationSourcesLikeness { get; set; } = new();
}

public class GenerationSource
{
    public required string Source;
    public double Likeness;
}