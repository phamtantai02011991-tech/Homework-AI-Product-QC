using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIProductQC.Core.Entities;
using AIProductQC.Core.Enums;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Data;
using AIProductQC.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIProductQC.Tests;

/// <summary>
/// Tests covering Mandatory Scenarios 01, 02, 03, 04, 07, 08, 09
/// for AI vision defect analysis, error handling, foreign key validation, and offline resilience.
/// </summary>
public class AiInspectionAnalysisTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IGeminiVisionService> _geminiMock;
    private readonly InspectionService _inspectionService;

    public AiInspectionAnalysisTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _fileStorageMock = new Mock<IFileStorageService>();
        _geminiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionService>>();

        // Default valid file storage mock
        string mime;
        string err;
        _fileStorageMock
            .Setup(f => f.ValidateImage(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(), out mime, out err))
            .Returns(new ValidateImageCallback((Stream s, string n, long l, out string outMime, out string outErr) =>
            {
                outMime = "image/jpeg";
                outErr = string.Empty;
                return true;
            }));

        _fileStorageMock
            .Setup(f => f.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/qc_test_sample.jpg");

        _fileStorageMock
            .Setup(f => f.GetImageBytesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0xFF, 0xD9 });

        _inspectionService = new InspectionService(
            _context,
            _fileStorageMock.Object,
            _geminiMock.Object,
            loggerMock.Object);
    }

    private delegate bool ValidateImageCallback(Stream stream, string name, long length, out string mimeType, out string error);

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task Scenario01_ClearImageOfGoodProduct_PassesWithoutInventoryMutation()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        _geminiMock.Setup(g => g.AnalyzeProductImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiAnalysisResult(
                CategoryName: "Không có lỗi (No Defect)",
                DefectDescription: "Sản phẩm đạt chuẩn hoàn thiện cao, bề mặt sáng bóng không trầy xước.",
                QualityStatus: QualityStatusValues.Passed,
                ConfidenceScore: 0.96,
                SeverityLevel: SeverityLevelValues.None,
                IsFallback: false
            ));

        using var stream = new MemoryStream(new byte[100]);

        // Act
        var (report, isFallback, _) = await _inspectionService.AnalyzeAndCreateDraftAsync(
            1, 1, stream, "good_product.jpg", 100);

        // Assert
        Assert.False(isFallback);
        Assert.Equal(QualityStatusValues.Passed, report.QualityStatus);
        Assert.Equal(0.96, report.ConfidenceScore);
        Assert.False(report.IsUserConfirmed);

        // Invariant: Inventory must remain untouched
        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 1);
        Assert.Equal(initialPassed, freshProduct.PassedStock);
        Assert.Equal(initialFailed, freshProduct.FailedStock);
    }

    [Fact]
    public async Task Scenario02_ImageWithMinorDefect_DetectsScratch_InventoryUntouched()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        _geminiMock.Setup(g => g.AnalyzeProductImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiAnalysisResult(
                CategoryName: "Trầy xước bề mặt (Surface Scratch)",
                DefectDescription: "Vết trầy mảnh dài 1.2cm trên bề mặt kính góc trên.",
                QualityStatus: QualityStatusValues.Failed,
                ConfidenceScore: 0.94,
                SeverityLevel: SeverityLevelValues.Low,
                IsFallback: false
            ));

        using var stream = new MemoryStream(new byte[100]);

        // Act
        var (report, isFallback, _) = await _inspectionService.AnalyzeAndCreateDraftAsync(
            1, 1, stream, "minor_scratch.jpg", 100);

        // Assert
        Assert.False(isFallback);
        Assert.Equal(QualityStatusValues.Failed, report.QualityStatus);
        Assert.Equal(2, report.DefectCategoryId);
        Assert.False(report.IsUserConfirmed);

        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 1);
        Assert.Equal(initialPassed, freshProduct.PassedStock);
        Assert.Equal(initialFailed, freshProduct.FailedStock);
    }

    [Fact]
    public async Task Scenario03_ImageWithMajorDefect_DetectsStructuralCrack()
    {
        // Arrange
        _geminiMock.Setup(g => g.AnalyzeProductImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiAnalysisResult(
                CategoryName: "Nứt gãy kết cấu (Structural Crack)",
                DefectDescription: "Đường nứt sâu kéo dài tại khớp chịu lực thân máy.",
                QualityStatus: QualityStatusValues.Failed,
                ConfidenceScore: 0.98,
                SeverityLevel: SeverityLevelValues.High,
                IsFallback: false
            ));

        using var stream = new MemoryStream(new byte[100]);

        // Act
        var (report, isFallback, _) = await _inspectionService.AnalyzeAndCreateDraftAsync(
            1, 1, stream, "major_crack.jpg", 100);

        // Assert
        Assert.False(isFallback);
        Assert.Equal(QualityStatusValues.Failed, report.QualityStatus);
        Assert.Equal(3, report.DefectCategoryId);
        Assert.False(report.IsUserConfirmed);
    }

    [Fact]
    public async Task Scenario04_BlurryImage_ReturnsNeedsReviewWithLowConfidence()
    {
        // Arrange
        _geminiMock.Setup(g => g.AnalyzeProductImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiAnalysisResult(
                CategoryName: "Không có lỗi (No Defect)",
                DefectDescription: "Ảnh chụp bị rung mờ, mất nét, không đủ độ tương phản để đánh giá bề mặt.",
                QualityStatus: QualityStatusValues.NeedsReview,
                ConfidenceScore: 0.40,
                SeverityLevel: SeverityLevelValues.None,
                IsFallback: false
            ));

        using var stream = new MemoryStream(new byte[100]);

        // Act
        var (report, isFallback, _) = await _inspectionService.AnalyzeAndCreateDraftAsync(
            1, 1, stream, "blurry.jpg", 100);

        // Assert
        Assert.Equal(QualityStatusValues.NeedsReview, report.QualityStatus);
        Assert.True(report.ConfidenceScore < 0.60);
        Assert.False(report.IsUserConfirmed);
    }

    [Fact]
    public async Task Scenario07_MissingProductId_ThrowsKeyNotFoundException()
    {
        // Arrange: Non-existent product ID 9999
        using var stream = new MemoryStream(new byte[100]);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _inspectionService.AnalyzeAndCreateDraftAsync(9999, 1, stream, "test.jpg", 100));
    }

    [Fact]
    public async Task Scenario08_MissingInspectorId_ThrowsArgumentException()
    {
        // Arrange: Non-existent inspector ID 9999
        using var stream = new MemoryStream(new byte[100]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inspectionService.AnalyzeAndCreateDraftAsync(1, 9999, stream, "test.jpg", 100));
    }

    [Fact]
    public async Task Scenario09_GeminiApiFailure_GracefullyDegradesToNeedsReview()
    {
        // Arrange: Gemini API throws network or rate limit failure, returning fallback result
        _geminiMock.Setup(g => g.AnalyzeProductImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiAnalysisResult(
                CategoryName: "Không có lỗi (No Defect)",
                DefectDescription: "Dịch vụ AI hiện không khả dụng. Chuyển sang chờ kiểm duyệt thủ công.",
                QualityStatus: QualityStatusValues.NeedsReview,
                ConfidenceScore: 0.0,
                SeverityLevel: SeverityLevelValues.None,
                IsFallback: true,
                ErrorMessage: "Gemini HTTP 429 Rate Limit"
            ));

        using var stream = new MemoryStream(new byte[100]);

        // Act: Should create unconfirmed draft in NeedsReview state without crashing
        var (report, isFallback, warningMessage) = await _inspectionService.AnalyzeAndCreateDraftAsync(
            1, 1, stream, "test.jpg", 100);

        // Assert
        Assert.True(isFallback);
        Assert.Equal(QualityStatusValues.NeedsReview, report.QualityStatus);
        Assert.Equal(0.0, report.ConfidenceScore);
        Assert.False(report.IsUserConfirmed);
        Assert.Contains("429", warningMessage);
    }
}
