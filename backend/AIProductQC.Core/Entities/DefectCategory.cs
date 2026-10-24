using System.Collections.Generic;

namespace AIProductQC.Core.Entities;

/// <summary>
/// Defines a defect typology category and its severity level.
/// </summary>
public class DefectCategory
{
    public int DefectCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string SeverityLevel { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Navigation property: DefectCategory 1:N InspectionReport
    public ICollection<InspectionReport> InspectionReports { get; set; } = new List<InspectionReport>();
}
