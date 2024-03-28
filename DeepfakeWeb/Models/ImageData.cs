using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeepfakeWeb.Data;

public class ImageData
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required Guid Id { get; set; }
    [MaxLength(255)] public required string FileName { get; set; }
    [MaxLength(255)] public required string ImageFileBase64 { get; set; }
}