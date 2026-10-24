using System;

namespace AIProductQC.Core.Entities;

/// <summary>
/// Represents a quality control inspection session and its AI diagnosis audit record.
/// </summary>
public class InspectionReport
{
    public int ReportId { get; set; }

    public int ProductId { get; set; }

    public int InspectorId { get; set; }

    public int DefectCategoryId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string DefectDescription { get; set; } = string.Empty;

    public string QualityStatus { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }

    public bool IsUserConfirmed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optimistic concurrency token
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product? Product { get; set; }

    public Inspector? Inspector { get; set; }

    public DefectCategory? DefectCategory { get; set; }
}
