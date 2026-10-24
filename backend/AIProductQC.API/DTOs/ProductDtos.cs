using System;
using System.ComponentModel.DataAnnotations;

namespace AIProductQC.API.DTOs;

public record ProductResponseDto(
    int ProductId,
    string ProductName,
    string Category,
    int TotalStock,
    int PassedStock,
    int FailedStock,
    int UninspectedStock,
    DateTime CreatedAt
);

public record ProductDetailResponseDto(
    int ProductId,
    string ProductName,
    string Category,
    int TotalStock,
    int PassedStock,
    int FailedStock,
    int UninspectedStock,
    double PassRatePercentage,
    DateTime CreatedAt,
    int RecentInspectionsCount
);

public class CreateProductRequestDto
{
    [Required(ErrorMessage = "ProductName is required.")]
    [StringLength(200, ErrorMessage = "ProductName cannot exceed 200 characters.")]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "TotalStock must be greater than or equal to 0.")]
    public int TotalStock { get; set; } = 0;
}
