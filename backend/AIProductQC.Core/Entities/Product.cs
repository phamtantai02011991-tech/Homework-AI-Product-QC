using System;
using System.Collections.Generic;

namespace AIProductQC.Core.Entities;

/// <summary>
/// Represents a registered manufactured product batch tracked by quality control.
/// </summary>
public class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int TotalStock { get; set; }

    public int PassedStock { get; set; }

    public int FailedStock { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: Product 1:N InspectionReport
    public ICollection<InspectionReport> InspectionReports { get; set; } = new List<InspectionReport>();
}
