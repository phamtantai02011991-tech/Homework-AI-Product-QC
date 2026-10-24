using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AIProductQC.API.DTOs;

public class AnalyzeInspectionRequestDto
{
    [Required(ErrorMessage = "ProductId is required.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "InspectorId is required.")]
    public int InspectorId { get; set; }

    [Required(ErrorMessage = "Image file is required.")]
    public IFormFile File { get; set; } = null!;
}

public record AnalyzeInspectionResponseDto(
    int ReportId,
    int ProductId,
    string ProductName,
    int InspectorId,
    string InspectorName,
    int DefectCategoryId,
    string DefectCategoryName,
    string SeverityLevel,
    string ImageUrl,
    string DefectDescription,
    string QualityStatus,
    double ConfidenceScore,
    bool IsUserConfirmed,
    DateTime CreatedAt,
    bool IsAiFallback,
    string? WarningMessage = null
);

public class ConfirmInspectionRequestDto
{
    public string? QualityStatus { get; set; }

    public int? DefectCategoryId { get; set; }

    public string? DefectDescription { get; set; }
}

public record ConfirmInspectionResponseDto(
    int ReportId,
    int ProductId,
    string ProductName,
    string QualityStatus,
    string DefectCategoryName,
    bool IsUserConfirmed,
    DateTime ConfirmedAt,
    ProductResponseDto UpdatedInventory,
    string Message
);

public record InspectionReportSummaryDto(
    int ReportId,
    int ProductId,
    string ProductName,
    int InspectorId,
    string InspectorName,
    int DefectCategoryId,
    string DefectCategoryName,
    string SeverityLevel,
    string ImageUrl,
    string QualityStatus,
    double ConfidenceScore,
    bool IsUserConfirmed,
    DateTime CreatedAt
);

public record InspectionReportDetailDto(
    int ReportId,
    ProductResponseDto Product,
    InspectorResponseDto Inspector,
    DefectCategoryResponseDto DefectCategory,
    string ImageUrl,
    string DefectDescription,
    string QualityStatus,
    double ConfidenceScore,
    bool IsUserConfirmed,
    DateTime CreatedAt
);

public record PaginatedListDto<T>(
    int TotalItems,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<T> Items
);
