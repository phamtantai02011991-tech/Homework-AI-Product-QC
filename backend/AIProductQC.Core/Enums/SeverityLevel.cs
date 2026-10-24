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
