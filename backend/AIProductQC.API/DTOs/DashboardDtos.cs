using System.Collections.Generic;

namespace AIProductQC.API.DTOs;

public record DefectCategoryStatDto(
    string CategoryName,
    int Count,
    double Percentage
);

public record DashboardSummaryDto(
    int TotalProducts,
    int TotalInspections,
    int TotalConfirmed,
    int TotalPendingConfirmation,
    int TotalPassed,
    int TotalFailed,
    double PassRate,
    IReadOnlyList<DefectCategoryStatDto> DefectDistribution,
    IReadOnlyList<InspectionReportSummaryDto> RecentInspections
);
