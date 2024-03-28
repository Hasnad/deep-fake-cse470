using Newtonsoft.Json;

namespace DeepfakeWeb.Models.JsonModel;

public class ImageAnalysisModel
{
    [JsonProperty("success")] public bool Success { get; set; }
    [JsonProperty("message")] public required string Message { get; set; }
    [JsonProperty("confidence")] public Confidence? Confidence { get; set; }
    [JsonProperty("face_with_mask")] public string FaceWithMask { get; set; }
}
public class Confidence
{
    public double Fake { get; set; }
    public double Real { get; set; }
}