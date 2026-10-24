namespace AIProductQC.API.DTOs;

public record DefectCategoryResponseDto(
    int DefectCategoryId,
    string CategoryName,
    string SeverityLevel,
    string Description
);
