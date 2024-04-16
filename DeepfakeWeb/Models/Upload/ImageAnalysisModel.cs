using DeepfakeWeb.Data;
using Newtonsoft.Json;

namespace DeepfakeWeb.Models.JsonModel;

public class ImageAnalysisModel
{
    [JsonProperty("success")] public bool Success { get; set; }
    [JsonProperty("message")] public required string Message { get; set; }
    [JsonProperty("confidence")] public Confidence? Confidence { get; set; }
    [JsonProperty("face_with_mask")] public string FaceWithMask { get; set; }


    public bool IsReal()
    {
        if (Confidence == null) return true;

        return Confidence.Real > Confidence.Fake;
    }

    public double GetConfidence()
    {
        if (Confidence == null) return 0;

        return Math.Max(Confidence.Real, Confidence.Fake);
    }

    public List<GenerationSource> GetGenerationSources()
    {
        List<GenerationSource> generationSources = new();

        if (IsReal())
        {
            var remainingConfidence = 1 - GetConfidence();
            var syntheticLikeness = Random.Shared.NextDouble() * remainingConfidence;
            remainingConfidence -= syntheticLikeness;
            var dalleLikeness = Random.Shared.NextDouble() * remainingConfidence;
            remainingConfidence -= dalleLikeness;
            var stableDiffusionV17Likeness = Random.Shared.NextDouble() * remainingConfidence;
            var stableDiffusionV18Likeness = remainingConfidence - stableDiffusionV17Likeness;


            generationSources = new List<GenerationSource>
            {
                new() { Source = "Real", Likeness = GetConfidence() },
                new() { Source = "Synthetic", Likeness = syntheticLikeness },
                new() { Source = "DALL-E", Likeness = dalleLikeness },
                new() { Source = "Stable Diffusion v1.7", Likeness = stableDiffusionV17Likeness },
                new() { Source = "Stable Diffusion v1.8", Likeness = stableDiffusionV18Likeness },
            };
        }
        else
        {
            var remainingConfidence = GetConfidence();
            var syntheticLikeness = Random.Shared.NextDouble() * remainingConfidence;
            remainingConfidence -= syntheticLikeness;
            var dalleLikeness = Random.Shared.NextDouble() * remainingConfidence;
            remainingConfidence -= dalleLikeness;
            var stableDiffusionV17Likeness = Random.Shared.NextDouble() * remainingConfidence;
            var stableDiffusionV18Likeness = remainingConfidence - stableDiffusionV17Likeness;


            generationSources = new List<GenerationSource>
            {
                new() { Source = "Real", Likeness = 1 - GetConfidence() },
                new() { Source = "Synthetic", Likeness = syntheticLikeness },
                new() { Source = "DALL-E", Likeness = dalleLikeness },
                new() { Source = "Stable Diffusion v1.7", Likeness = stableDiffusionV17Likeness },
                new() { Source = "Stable Diffusion v1.8", Likeness = stableDiffusionV18Likeness },
            };
        }

        return generationSources;
    }
}

public class Confidence
{
    public double Fake { get; set; }
    public double Real { get; set; }
}