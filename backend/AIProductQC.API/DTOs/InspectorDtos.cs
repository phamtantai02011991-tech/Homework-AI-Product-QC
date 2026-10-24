namespace AIProductQC.API.DTOs;

public record InspectorResponseDto(
    int InspectorId,
    string InspectorName,
    string Department,
    string Email
);
