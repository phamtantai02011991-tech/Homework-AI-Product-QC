using System.Collections.Generic;

namespace AIProductQC.Core.Entities;

/// <summary>
/// Represents an authorized QC inspector.
/// </summary>
public class Inspector
{
    public int InspectorId { get; set; }

    public string InspectorName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Navigation property: Inspector 1:N InspectionReport
    public ICollection<InspectionReport> InspectionReports { get; set; } = new List<InspectionReport>();
}
