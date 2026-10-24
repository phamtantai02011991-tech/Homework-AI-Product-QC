using System.Threading;
using System.Threading.Tasks;

namespace AIProductQC.Core.Interfaces;

/// <summary>
/// Structured result returned from Gemini Vision Multimodal AI inspection.
/// </summary>
public record AiAnalysisResult(
    string CategoryName,
    string DefectDescription,
    string QualityStatus,
    double ConfidenceScore,
    string SeverityLevel,
    bool IsFallback = false,
    string? ErrorMessage = null
);

/// <summary>
/// Service contract for Gemini Vision Multimodal AI defect analysis.
/// </summary>
public interface IGeminiVisionService
{
    /// <summary>
    /// Analyzes an image of a product using Gemini Multimodal Vision API.
    /// </summary>
    /// <param name="imageBytes">Raw image binary data.</param>
    /// <param name="mimeType">Image MIME type (e.g. image/jpeg, image/png).</param>
    /// <param name="productName">Product name context for AI prompt.</param>
    /// <param name="category">Product category context for AI prompt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Structured analysis result with Vietnamese defect description.</returns>
    Task<AiAnalysisResult> AnalyzeProductImageAsync(
        byte[] imageBytes,
        string mimeType,
        string? productName = null,
        string? category = null,
        CancellationToken cancellationToken = default);
}
