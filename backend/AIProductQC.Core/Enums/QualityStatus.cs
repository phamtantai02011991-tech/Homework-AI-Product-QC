namespace AIProductQC.Core.Enums;

/// <summary>
/// Represents the quality outcome of an inspection.
/// </summary>
public enum QualityStatus
{
    /// <summary>
    /// Product passes all quality checks with no defects detected.
    /// </summary>
    Passed,

    /// <summary>
    /// Product failed inspection due to detected defects.
    /// </summary>
    Failed,

    /// <summary>
    /// Image is blurry, unclear, or AI confidence is low; manual review is required.
    /// </summary>
    NeedsReview,

    /// <summary>
    /// Initial pending state before AI analysis completes.
    /// </summary>
    Pending
}
