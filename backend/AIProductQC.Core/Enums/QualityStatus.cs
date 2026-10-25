using System;

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

/// <summary>
/// Strongly-typed constant string values and helper methods for QualityStatus.
/// </summary>
public static class QualityStatusValues
{
    public const string Passed = nameof(QualityStatus.Passed);
    public const string Failed = nameof(QualityStatus.Failed);
    public const string NeedsReview = nameof(QualityStatus.NeedsReview);
    public const string Pending = nameof(QualityStatus.Pending);

    public static bool TryParse(string? value, out QualityStatus status)
    {
        return Enum.TryParse(value, ignoreCase: true, out status);
    }

    public static bool IsValid(string? value)
    {
        return Enum.TryParse<QualityStatus>(value, ignoreCase: true, out _);
    }
}
