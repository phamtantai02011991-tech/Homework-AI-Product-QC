using System;
using System.IO;
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
/// Mandatory Scenarios 11, 12, 13, 14, 15:
/// Tests atomic transactions, rollback guarantees, duplicate confirmation prevention,
/// and concurrency race condition safety using SQLite in-memory transactions.
/// </summary>
public class ConfirmationTransactionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly InspectionService _service;

    public ConfirmationTransactionTests()
    {
        // Use SQLite in-memory for real ACID transaction execution
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        var fileMock = new Mock<IFileStorageService>();
        var aiMock = new Mock<IGeminiVisionService>();
        var loggerMock = new Mock<ILogger<InspectionService>>();

        _service = new InspectionService(_context, fileMock.Object, aiMock.Object, loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private async Task<InspectionReport> CreateDraftReportAsync(int productId, string initialStatus = "Passed", int categoryId = 1)
    {
        var report = new InspectionReport
        {
            ProductId = productId,
            InspectorId = 1,
            DefectCategoryId = categoryId,
            ImageUrl = "/uploads/qc_test_sample.jpg",
            DefectDescription = "Bản nháp chờ QC xác nhận.",
            QualityStatus = initialStatus,
            ConfidenceScore = 0.90,
            IsUserConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.InspectionReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    [Fact]
    public async Task Scenario11_ConfirmInspection_PassedReport_AtomicallyIncrementsPassedStock()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        var draft = await CreateDraftReportAsync(product.ProductId, QualityStatusValues.Passed, 1);

        // Act: Confirm report as Passed
        var (confirmedReport, updatedProduct, msg) = await _service.ConfirmInspectionAsync(
            draft.ReportId, QualityStatusValues.Passed, null, null);

        // Assert
        Assert.True(confirmedReport.IsUserConfirmed);
        Assert.Equal(QualityStatusValues.Passed, confirmedReport.QualityStatus);
        Assert.Equal(initialPassed + 1, updatedProduct.PassedStock);
        Assert.Equal(initialFailed, updatedProduct.FailedStock);
        Assert.Contains("PassedStock", msg);

        // Verify persistence in database
        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 1);
        Assert.Equal(initialPassed + 1, freshProduct.PassedStock);
    }

    [Fact]
    public async Task Scenario12_ConfirmInspection_FailedReport_AtomicallyIncrementsFailedStock()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 2);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        var draft = await CreateDraftReportAsync(product.ProductId, QualityStatusValues.Failed, 2);

        // Act: Confirm report as Failed
        var (confirmedReport, updatedProduct, msg) = await _service.ConfirmInspectionAsync(
            draft.ReportId, QualityStatusValues.Failed, 2, "Xác nhận có vết xước sâu");

        // Assert
        Assert.True(confirmedReport.IsUserConfirmed);
        Assert.Equal(QualityStatusValues.Failed, confirmedReport.QualityStatus);
        Assert.Equal(2, confirmedReport.DefectCategoryId);
        Assert.Equal(initialPassed, updatedProduct.PassedStock);
        Assert.Equal(initialFailed + 1, updatedProduct.FailedStock);
        Assert.Contains("FailedStock", msg);

        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 2);
        Assert.Equal(initialFailed + 1, freshProduct.FailedStock);
    }

    [Fact]
    public async Task Scenario13_RepeatedConfirmation_ThrowsInvalidOperationException_StockNotDoubleCounted()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var draft = await CreateDraftReportAsync(product.ProductId, QualityStatusValues.Passed);

        // First confirmation succeeds
        await _service.ConfirmInspectionAsync(draft.ReportId, QualityStatusValues.Passed, null, null);

        var stockAfterFirst = (await _context.Products.FirstAsync(p => p.ProductId == 1)).PassedStock;

        // Act & Assert: Second confirmation MUST throw and fail
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ConfirmInspectionAsync(draft.ReportId, QualityStatusValues.Passed, null, null));

        Assert.Contains("already been confirmed", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Invariant: Stock must not be double counted
        var stockAfterSecond = (await _context.Products.FirstAsync(p => p.ProductId == 1)).PassedStock;
        Assert.Equal(stockAfterFirst, stockAfterSecond);
    }

    [Fact]
    public async Task Scenario14_TransactionRollbackOnFailure_GuaranteesNoPartialMutation()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var initialFailed = product.FailedStock;

        var draft = await CreateDraftReportAsync(product.ProductId, QualityStatusValues.Passed);

        // Act & Assert: Provide invalid category ID 99999 inside transaction
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ConfirmInspectionAsync(draft.ReportId, QualityStatusValues.Failed, 99999, null));

        // Invariant: All changes must roll back completely
        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 1);
        Assert.Equal(initialPassed, freshProduct.PassedStock);
        Assert.Equal(initialFailed, freshProduct.FailedStock);

        var freshReport = await _context.InspectionReports.FirstAsync(r => r.ReportId == draft.ReportId);
        Assert.False(freshReport.IsUserConfirmed);
    }

    [Fact]
    public async Task Scenario15_ConcurrentConfirmationAttempts_OnlyOneSucceeds()
    {
        // Arrange
        var product = await _context.Products.FirstAsync(p => p.ProductId == 1);
        var initialPassed = product.PassedStock;
        var draft = await CreateDraftReportAsync(product.ProductId, QualityStatusValues.Passed);

        // Act: Fire two concurrent confirmation tasks
        var task1 = _service.ConfirmInspectionAsync(draft.ReportId, QualityStatusValues.Passed, null, null);
        var task2 = _service.ConfirmInspectionAsync(draft.ReportId, QualityStatusValues.Passed, null, null);

        var results = await Task.WhenAll(
            task1.ContinueWith(t => t.IsCompletedSuccessfully),
            task2.ContinueWith(t => t.IsCompletedSuccessfully)
        );

        // Assert: Exactly one must succeed, the other must fail with conflict/exception
        int successCount = (results[0] ? 1 : 0) + (results[1] ? 1 : 0);
        Assert.Equal(1, successCount);

        // Invariant: Stock increments by exactly 1, never 2
        var freshProduct = await _context.Products.FirstAsync(p => p.ProductId == 1);
        Assert.Equal(initialPassed + 1, freshProduct.PassedStock);
    }
}
