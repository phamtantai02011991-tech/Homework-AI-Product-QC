using System;
using System.Net.Http;
using AIProductQC.Core.Enums;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIProductQC.Tests;

/// <summary>
/// Mandatory Test Scenario 10: Robustness testing for Gemini Vision AI response parsing,
/// including markdown code fences, malformed JSON, missing fields, and non-JSON text.
/// </summary>
public class GeminiVisionParsingTests
{
    private readonly GeminiVisionService _service;

    public GeminiVisionParsingTests()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Gemini:Model"]).Returns("gemini-3.5-flash");
        var loggerMock = new Mock<ILogger<GeminiVisionService>>();
        var httpClient = new HttpClient();

        _service = new GeminiVisionService(httpClient, configMock.Object, loggerMock.Object);
    }

    private static string BuildGeminiCandidateResponse(string innerText)
    {
        var obj = new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = innerText }
                        }
                    }
                }
            }
        };
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }

    [Fact]
    public void Scenario10_ParseGeminiResponse_ValidMarkdownFencedJson_ParsesCorrectly()
    {
        // Arrange: Valid JSON wrapped in markdown ```json ... ``` block
        var jsonPayload = @"{
          ""categoryName"": ""Trầy xước bề mặt (Surface Scratch)"",
          ""defectDescription"": ""Phát hiện vết xước mảnh 1.2cm trên bề mặt nhôm anode."",
          ""qualityStatus"": ""Failed"",
          ""confidenceScore"": 0.94,
          ""severityLevel"": ""Low""
        }";

        var geminiResponse = BuildGeminiCandidateResponse($"```json\n{jsonPayload}\n```");

        // Act
        var result = _service.ParseGeminiResponse(geminiResponse);

        // Assert
        Assert.False(result.IsFallback);
        Assert.Equal("Trầy xước bề mặt (Surface Scratch)", result.CategoryName);
        Assert.Equal("Failed", result.QualityStatus);
        Assert.Equal(0.94, result.ConfidenceScore);
        Assert.Equal("Low", result.SeverityLevel);
        Assert.Contains("vết xước mảnh", result.DefectDescription);
    }

    [Fact]
    public void Scenario10_ParseGeminiResponse_MalformedJson_GracefullyFallsBackToNeedsReview()
    {
        // Arrange: Completely broken JSON inside response text
        var brokenText = "{ categoryName: Trầy xước, defectDescription: broken json without quotes ";
        var geminiResponse = BuildGeminiCandidateResponse(brokenText);

        // Act: Should NOT throw an unhandled JsonException
        var result = _service.ParseGeminiResponse(geminiResponse);

        // Assert
        Assert.True(result.IsFallback);
        Assert.Equal(QualityStatusValues.NeedsReview, result.QualityStatus);
        Assert.Equal(0.0, result.ConfidenceScore);
        Assert.Equal("Không có lỗi (No Defect)", result.CategoryName);
        Assert.Contains("JSON", result.DefectDescription);
    }

    [Fact]
    public void Scenario10_ParseGeminiResponse_EmptyCandidates_GracefullyFallsBackToNeedsReview()
    {
        // Arrange: Gemini returns empty candidates array
        var geminiResponse = @"{ ""candidates"": [] }";

        // Act
        var result = _service.ParseGeminiResponse(geminiResponse);

        // Assert
        Assert.True(result.IsFallback);
        Assert.Equal(QualityStatusValues.NeedsReview, result.QualityStatus);
        Assert.Equal(0.0, result.ConfidenceScore);
    }

    [Fact]
    public void Scenario10_ParseGeminiResponse_MissingFields_DefaultsSafely()
    {
        // Arrange: Valid JSON object but missing qualityStatus and confidenceScore
        var jsonPayload = @"{
          ""categoryName"": ""Nứt gãy kết cấu (Structural Crack)"",
          ""defectDescription"": ""Vết nứt chân đế.""
        }";

        var geminiResponse = BuildGeminiCandidateResponse(jsonPayload);

        // Act
        var result = _service.ParseGeminiResponse(geminiResponse);

        // Assert
        Assert.False(result.IsFallback);
        Assert.Equal("Nứt gãy kết cấu (Structural Crack)", result.CategoryName);
        Assert.Equal("NeedsReview", result.QualityStatus); // Default when unspecified
        Assert.InRange(result.ConfidenceScore, 0.0, 1.0);
    }
}
