using System;

namespace AIProductQC.Core.Enums;

/// <summary>
/// Defines severity levels for classified defects.
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// Flawless product with no defects.
    /// </summary>
    None,

    /// <summary>
    /// Minor surface blemishes, scratches, or cosmetic marks that do not impact functionality.
    /// </summary>
    Low,

    /// <summary>
    /// Noticeable dent, deformation, or discoloration affecting visual quality or assembly fit.
    /// </summary>
    Medium,

    /// <summary>
    /// Structural crack, severe deformation, or major assembly error causing rejection.
    /// </summary>
    High,

    /// <summary>
    /// Dangerous failure risking safety, electrical hazard, or structural integrity loss.
    /// </summary>
    Critical
}

/// <summary>
/// Strongly-typed constant string values and helper methods for SeverityLevel.
/// </summary>
public static class SeverityLevelValues
{
    public const string None = nameof(SeverityLevel.None);
    public const string Low = nameof(SeverityLevel.Low);
    public const string Medium = nameof(SeverityLevel.Medium);
    public const string High = nameof(SeverityLevel.High);
    public const string Critical = nameof(SeverityLevel.Critical);

    public static bool TryParse(string? value, out SeverityLevel level)
    {
        return Enum.TryParse(value, ignoreCase: true, out level);
    }
}
